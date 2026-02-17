import argparse
import asyncio
import base64
import csv
import json
import math
import os
import random
import textwrap
from dataclasses import dataclass
from datetime import date, datetime, timedelta
from pathlib import Path




def load_env_file(path: str = ".env") -> None:
    env_path = Path(path)
    if not env_path.exists():
        return
    for line in env_path.read_text(encoding="utf-8").splitlines():
        item = line.strip()
        if not item or item.startswith("#") or "=" not in item:
            continue
        key, value = item.split("=", 1)
        key = key.strip()
        value = value.strip().strip('"').strip("'")
        if key and key not in os.environ:
            os.environ[key] = value
WEEKDAY_MAP = {"mon": 0, "tue": 1, "wed": 2, "thu": 3, "fri": 4, "sat": 5, "sun": 6}


@dataclass
class Config:
    openai_api_key: str
    pexels_api_key: str
    voice: str
    duration_seconds: int
    width: int
    height: int
    fps: int
    visual_mode: str
    shot_count: int


@dataclass
class Shot:
    index: int
    start: float
    end: float
    prompt: str

    @property
    def duration(self) -> float:
        return max(0.25, self.end - self.start)


@dataclass
class ChannelPlan:
    channel: str
    posts_per_week: int
    days: list[int]
    time_slots: list[str]
    topics: list[str]
    priority: int = 1


def parse_list(value: str, default: list[str]) -> list[str]:
    if not value:
        return default
    values = [v.strip() for v in value.split(",") if v.strip()]
    return values or default


def parse_weekdays(days: list[str]) -> list[int]:
    parsed: list[int] = []
    for item in days:
        key = item.lower()[:3]
        if key not in WEEKDAY_MAP:
            raise RuntimeError(f"Unsupported weekday '{item}'. Use mon,tue,wed,thu,fri,sat,sun.")
        parsed.append(WEEKDAY_MAP[key])
    ordered = sorted(set(parsed))
    return ordered if ordered else [0, 2, 4]


def load_config() -> Config:
    load_env_file()

    openai_api_key = os.getenv("OPENAI_API_KEY", "").strip()
    pexels_api_key = os.getenv("PEXELS_API_KEY", "").strip()
    voice = os.getenv("VOICE", "en-US-GuyNeural").strip()
    duration_seconds = int(os.getenv("VIDEO_DURATION_SECONDS", "45"))
    fps = int(os.getenv("VIDEO_FPS", "30"))
    visual_mode = os.getenv("VISUAL_MODE", "image").strip().lower()
    shot_count = int(os.getenv("SHOT_COUNT", "8"))

    size = os.getenv("VIDEO_SIZE", "1080x1920").lower().split("x")
    width, height = int(size[0]), int(size[1])

    if visual_mode not in {"image", "video"}:
        raise RuntimeError("VISUAL_MODE must be 'image' or 'video'")

    if visual_mode == "video" and not pexels_api_key:
        raise RuntimeError("Missing PEXELS_API_KEY for VISUAL_MODE=video")

    return Config(openai_api_key, pexels_api_key, voice, duration_seconds, width, height, fps, visual_mode, shot_count)


def generate_script(topic: str, api_key: str, target_seconds: int) -> str:
    if not api_key:
        raise RuntimeError("Missing OPENAI_API_KEY")

    from openai import OpenAI

    words = max(80, math.floor(target_seconds * 2.4))
    client = OpenAI(api_key=api_key)
    prompt = (
        f"Write an engaging short-form faceless video narration about '{topic}'. "
        f"Return around {words} words. Plain text only, no markdown, no headings."
    )

    response = client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[
            {"role": "system", "content": "You write concise viral narration scripts."},
            {"role": "user", "content": prompt},
        ],
        temperature=0.9,
    )
    return response.choices[0].message.content.strip()


def generate_shot_plan(narration: str, api_key: str, shot_count: int, duration_seconds: int) -> list[Shot]:
    if not api_key:
        data = []
    else:
        from openai import OpenAI

        client = OpenAI(api_key=api_key)
        prompt = (
            "Create a visual shot plan for this short-form narration. "
            f"Return EXACT JSON array with {shot_count} objects containing prompt/start/end. "
            f"Timeline must cover 0..{duration_seconds} seconds.\n\nNarration:\n{narration}"
        )
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": "You are a precise storyboard planner."},
                {"role": "user", "content": prompt},
            ],
            temperature=0.4,
        )
        raw = (response.choices[0].message.content or "").strip()
        try:
            data = json.loads(raw)
        except json.JSONDecodeError:
            data = []

    chunk = duration_seconds / max(1, shot_count)
    shots: list[Shot] = []
    for i in range(shot_count):
        item = data[i] if i < len(data) and isinstance(data[i], dict) else {}
        start = float(item.get("start", i * chunk))
        end = float(item.get("end", (i + 1) * chunk))
        prompt = str(item.get("prompt", f"Cinematic vertical scene about: {narration[:120]}"))
        shots.append(Shot(i + 1, max(0.0, start), max(start + 0.25, end), prompt))

    shots[0].start = 0.0
    shots[-1].end = float(duration_seconds)
    return shots


async def synthesize_tts(text: str, voice: str, output_audio: Path) -> None:
    import edge_tts

    communicate = edge_tts.Communicate(text=text, voice=voice)
    await communicate.save(str(output_audio))


def generate_images_openai(shots: list[Shot], api_key: str, out_dir: Path, size: str = "1024x1536") -> list[Path]:
    if not api_key:
        raise RuntimeError("Missing OPENAI_API_KEY for VISUAL_MODE=image")

    from openai import OpenAI

    out_dir.mkdir(parents=True, exist_ok=True)
    client = OpenAI(api_key=api_key)
    paths: list[Path] = []

    for shot in shots:
        prompt = f"Vertical cinematic frame. {shot.prompt}. Realistic lighting, no text overlay."
        response = client.images.generate(model="gpt-image-1", prompt=prompt, size=size)
        target = out_dir / f"shot_{shot.index:02d}.png"
        target.write_bytes(base64.b64decode(response.data[0].b64_json))
        paths.append(target)

    return paths


def search_pexels_clips(query: str, api_key: str, min_count: int = 8) -> list[str]:
    import requests

    res = requests.get(
        "https://api.pexels.com/videos/search",
        headers={"Authorization": api_key},
        params={"query": query, "per_page": 40, "orientation": "portrait"},
        timeout=30,
    )
    res.raise_for_status()

    picks: list[str] = []
    for video in res.json().get("videos", []):
        files = sorted(video.get("video_files", []), key=lambda x: x.get("width", 0))
        if files:
            picks.append(files[-1].get("link"))
        if len(picks) >= min_count:
            break

    return [p for p in picks if p]


def download_files(urls: list[str], folder: Path, ext: str = "mp4") -> list[Path]:
    import requests

    folder.mkdir(parents=True, exist_ok=True)
    out: list[Path] = []
    for i, url in enumerate(urls, start=1):
        target = folder / f"clip_{i:02d}.{ext}"
        with requests.get(url, stream=True, timeout=60) as res:
            res.raise_for_status()
            with target.open("wb") as f:
                for chunk in res.iter_content(chunk_size=1024 * 1024):
                    if chunk:
                        f.write(chunk)
        out.append(target)
    return out


def add_subtitle_overlay(video, narration: str, width: int, height: int, duration: float):
    from moviepy.editor import CompositeVideoClip, TextClip

    subtitle = TextClip(
        txt="\n".join(textwrap.wrap(narration[:260], width=36)),
        fontsize=52,
        color="white",
        stroke_color="black",
        stroke_width=2,
        font="DejaVu-Sans",
        method="caption",
        size=(width - 80, None),
    ).set_position(("center", "bottom")).set_duration(min(6, duration)).set_start(0.5)

    return CompositeVideoClip([video, subtitle], size=(width, height)).set_duration(duration)


def build_video_from_video_clips(
    clip_paths: list[Path], audio_path: Path, narration: str, output_path: Path, width: int, height: int, fps: int
) -> None:
    from moviepy.editor import AudioFileClip, VideoFileClip, concatenate_videoclips

    audio = AudioFileClip(str(audio_path))
    duration = audio.duration
    random.shuffle(clip_paths)
    selected = []
    running = 0.0

    for path in clip_paths:
        clip = VideoFileClip(str(path)).without_audio().resize(height=height)
        if clip.w < width:
            clip = clip.resize(width=width)

        x1 = max((clip.w - width) / 2, 0)
        y1 = max((clip.h - height) / 2, 0)
        clip = clip.crop(x1=x1, y1=y1, x2=x1 + width, y2=y1 + height)
        if running + clip.duration > duration:
            clip = clip.subclip(0, max(0.1, duration - running))

        selected.append(clip.crossfadein(0.2) if selected else clip)
        running += clip.duration
        if running >= duration:
            break

    if not selected:
        raise RuntimeError("No valid clips available to render video")

    video = concatenate_videoclips(selected, method="compose", padding=-0.2).set_audio(audio)
    final = add_subtitle_overlay(video, narration, width, height, duration)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    final.write_videofile(str(output_path), fps=fps, codec="libx264", audio_codec="aac", threads=4, preset="medium")
    audio.close()
    final.close()


def build_video_from_images(
    image_paths: list[Path],
    shots: list[Shot],
    audio_path: Path,
    narration: str,
    output_path: Path,
    width: int,
    height: int,
    fps: int,
) -> None:
    from moviepy.editor import AudioFileClip, ImageClip, concatenate_videoclips, vfx

    if not image_paths:
        raise RuntimeError("No generated images available to build video")

    audio = AudioFileClip(str(audio_path))
    clips = []

    for image_path, shot in zip(image_paths, shots):
        duration = shot.duration
        clip = ImageClip(str(image_path)).set_duration(duration).resize(height=height)
        clip = clip.fx(vfx.resize, lambda t: 1 + 0.04 * (t / max(duration, 0.01)))
        if clip.w < width:
            clip = clip.resize(width=width)
        x1 = max((clip.w - width) / 2, 0)
        y1 = max((clip.h - height) / 2, 0)
        clip = clip.crop(x1=x1, y1=y1, x2=x1 + width, y2=y1 + height)
        clips.append(clip.crossfadein(0.2) if clips else clip)

    video = concatenate_videoclips(clips, method="compose", padding=-0.2).set_audio(audio)
    final = add_subtitle_overlay(video, narration, width, height, audio.duration)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    final.write_videofile(str(output_path), fps=fps, codec="libx264", audio_codec="aac", threads=4, preset="medium")
    audio.close()
    final.close()


def build_channel_plans(
    channels: int, default_posts_per_week: int, default_days: list[int], default_times: list[str], topic_pool: list[str]
) -> list[ChannelPlan]:
    return [
        ChannelPlan(
            channel=f"channel_{idx+1:03d}",
            posts_per_week=default_posts_per_week,
            days=default_days,
            time_slots=default_times,
            topics=topic_pool,
            priority=1,
        )
        for idx in range(channels)
    ]


def load_channel_plans_from_json(path: Path, fallback: list[ChannelPlan]) -> list[ChannelPlan]:
    if not path.exists():
        return fallback

    raw = json.loads(path.read_text())
    plans: list[ChannelPlan] = []
    for item in raw:
        channel = str(item.get("channel", "")).strip()
        if not channel:
            continue
        plans.append(
            ChannelPlan(
                channel=channel,
                posts_per_week=int(item.get("posts_per_week", 3)),
                days=parse_weekdays([str(v) for v in item.get("days", ["mon", "wed", "fri"])]),
                time_slots=[str(v) for v in item.get("time_slots", ["09:00", "14:00", "19:00"])],
                topics=[str(v) for v in item.get("topics", ["general"])],
                priority=int(item.get("priority", 1)),
            )
        )
    return plans or fallback


def next_weekday(anchor: date, weekday: int) -> date:
    return anchor + timedelta(days=(weekday - anchor.weekday()) % 7)


def create_calendar(plans: list[ChannelPlan], start_date: date, weeks: int, timezone: str, out_csv: Path) -> None:
    rows: list[dict[str, str]] = []

    for plan in plans:
        topic_index = 0
        for week_index in range(weeks):
            anchor = start_date + timedelta(days=7 * week_index)
            slots = []
            for weekday in plan.days:
                day_date = next_weekday(anchor, weekday)
                for time in plan.time_slots:
                    slots.append((day_date, time))
            slots = sorted(slots, key=lambda x: (x[0], x[1]))[: max(1, plan.posts_per_week)]

            for post_num, (publish_date, publish_time) in enumerate(slots, start=1):
                topic = plan.topics[topic_index % max(1, len(plan.topics))]
                topic_index += 1
                rows.append(
                    {
                        "channel": plan.channel,
                        "priority": str(plan.priority),
                        "week": str(week_index + 1),
                        "post_index": str(post_num),
                        "publish_date": publish_date.isoformat(),
                        "publish_time": publish_time,
                        "timezone": timezone,
                        "topic": topic,
                        "status": "planned",
                    }
                )

    rows.sort(key=lambda r: (r["publish_date"], r["publish_time"], r["channel"]))
    out_csv.parent.mkdir(parents=True, exist_ok=True)
    with out_csv.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=[
                "channel",
                "priority",
                "week",
                "post_index",
                "publish_date",
                "publish_time",
                "timezone",
                "topic",
                "status",
            ],
        )
        writer.writeheader()
        writer.writerows(rows)


def write_channel_template(path: Path, plans: list[ChannelPlan]) -> None:
    reverse_day = {v: k for k, v in WEEKDAY_MAP.items()}
    payload = [
        {
            "channel": p.channel,
            "posts_per_week": p.posts_per_week,
            "days": [reverse_day[d] for d in p.days],
            "time_slots": p.time_slots,
            "topics": p.topics,
            "priority": p.priority,
        }
        for p in plans
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2))


def run_render(args) -> None:
    cfg = load_config()
    print("[1/6] Generating narration script...")
    narration = generate_script(args.topic, cfg.openai_api_key, cfg.duration_seconds)

    work = Path("work")
    work.mkdir(exist_ok=True)

    print("[2/6] Building shot plan...")
    shots = generate_shot_plan(narration, cfg.openai_api_key, cfg.shot_count, cfg.duration_seconds)

    print("[3/6] Synthesizing voiceover...")
    audio_path = work / "voiceover.mp3"
    asyncio.run(synthesize_tts(narration, cfg.voice, audio_path))

    if cfg.visual_mode == "image":
        print("[4/6] Generating AI images for each shot...")
        images = generate_images_openai(shots, cfg.openai_api_key, work / "images")
        print("[5/6] Rendering cinematic image-to-video sequence...")
        build_video_from_images(images, shots, audio_path, narration, Path(args.out), cfg.width, cfg.height, cfg.fps)
    else:
        print("[4/6] Searching stock video clips...")
        clip_urls = search_pexels_clips(args.topic, cfg.pexels_api_key, min_count=max(6, cfg.shot_count))
        if not clip_urls:
            raise RuntimeError("No clips found on Pexels for this query")
        print(f"[5/6] Downloading {len(clip_urls)} clips...")
        clips = download_files(clip_urls, work / "clips")
        print("[6/6] Rendering final video from clips...")
        build_video_from_video_clips(clips, audio_path, narration, Path(args.out), cfg.width, cfg.height, cfg.fps)

    print(f"Done: {args.out}")


def run_calendar(args) -> None:
    load_env_file()
    default_posts = int(os.getenv("CALENDAR_DEFAULT_POSTS_PER_WEEK", "3"))
    default_days = parse_weekdays(parse_list(os.getenv("CALENDAR_DEFAULT_DAYS", "mon,wed,fri"), ["mon", "wed", "fri"]))
    default_times = parse_list(os.getenv("CALENDAR_DEFAULT_TIMES", "09:00,14:00,19:00"), ["09:00", "14:00", "19:00"])
    timezone = args.timezone or os.getenv("CALENDAR_TIMEZONE", "UTC")

    topic_pool = parse_list(
        args.topics or os.getenv("CALENDAR_TOPICS", "motivation,productivity,finance,mindset,ai tools"), ["general"]
    )

    plans = build_channel_plans(args.channels, default_posts, default_days, default_times, topic_pool)
    if args.channels_file:
        plans = load_channel_plans_from_json(Path(args.channels_file), plans)

    create_calendar(plans, datetime.strptime(args.start_date, "%Y-%m-%d").date(), args.weeks, timezone, Path(args.out))

    if args.template_out:
        write_channel_template(Path(args.template_out), plans)

    print(f"Calendar saved to {args.out} for {len(plans)} channel(s) across {args.weeks} week(s).")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Faceless content pipeline: render videos + schedule channels")
    subparsers = parser.add_subparsers(dest="command", required=True)

    render = subparsers.add_parser("render", help="Generate and render one faceless video")
    render.add_argument("--topic", required=True, help="Video topic")
    render.add_argument("--out", default="output/video.mp4", help="Output video file path")

    cal = subparsers.add_parser("calendar", help="Generate scalable posting calendar")
    cal.add_argument("--channels", type=int, default=100, help="Number of channels to schedule")
    cal.add_argument("--weeks", type=int, default=4, help="Number of weeks to plan")
    cal.add_argument("--start-date", default=date.today().isoformat(), help="Start date YYYY-MM-DD")
    cal.add_argument("--timezone", default="", help="Timezone label for output rows")
    cal.add_argument("--topics", default="", help="Comma-separated reusable topic pool")
    cal.add_argument("--channels-file", default="", help="Optional JSON with per-channel settings")
    cal.add_argument("--template-out", default="", help="Optional path to write channel template JSON")
    cal.add_argument("--out", default="output/calendar.csv", help="Output calendar CSV path")

    return parser


def main() -> None:
    args = build_parser().parse_args()
    if args.command == "render":
        run_render(args)
    elif args.command == "calendar":
        run_calendar(args)
    else:
        raise RuntimeError("Unsupported command")


if __name__ == "__main__":
    main()
