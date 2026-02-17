# Automate Faceless Content (Enhanced Replica)

This repository is an enhanced, customizable replication inspired by:

- https://github.com/cporter202/automate-faceless-content
- https://github.com/SkyworkAI/SkyReels-V2

It includes:

1. **Video pipeline** (script → shot plan → voice → visual render).
2. **Scalable calendar generator** for up to 100+ channels.
3. **Packaging helpers** to build a Windows EXE and Android APK.

## Commands

### Render a video

```bash
python main.py render --topic "3 Stoic habits for better focus" --out output/video.mp4
```

### Generate schedule calendar (100 channels example)

```bash
python main.py calendar --channels 100 --weeks 8 --start-date 2026-02-01 --out output/calendar.csv
```

## Calendar customization

Use per-channel JSON:

```bash
python main.py calendar --channels-file channels.json --weeks 12 --out output/calendar.csv
```

Generate a template and edit it:

```bash
python main.py calendar --channels 100 --template-out output/channels.template.json
```

Example `channels.json`:

```json
[
  {
    "channel": "finance_daily",
    "posts_per_week": 5,
    "days": ["mon", "tue", "wed", "thu", "fri"],
    "time_slots": ["08:00", "12:00", "18:00"],
    "topics": ["budgeting", "investing", "side hustles"],
    "priority": 2
  }
]
```

## Build as EXE and APK

### Windows EXE

Run on Windows (or in a Windows CI runner):

```bash
bash scripts/build_exe.sh
```

Output:

- `dist/FacelessStudio.exe` (on Windows)

### Android APK

Run on Linux with Android build prerequisites installed (SDK/NDK/JDK):

```bash
bash scripts/build_apk.sh
```

Output:

- `bin/*.apk`

Files added for packaging:

- `scripts/build_exe.sh`
- `scripts/build_apk.sh`
- `buildozer.spec`
- `requirements-build.txt`

## Environment variables

Video pipeline:

- `OPENAI_API_KEY`
- `PEXELS_API_KEY` (required only when `VISUAL_MODE=video`)
- `VISUAL_MODE=image|video`
- `VOICE=en-US-GuyNeural`
- `VIDEO_DURATION_SECONDS=45`
- `VIDEO_SIZE=1080x1920`
- `VIDEO_FPS=30`
- `SHOT_COUNT=8`

Calendar defaults:

- `CALENDAR_DEFAULT_POSTS_PER_WEEK=3`
- `CALENDAR_DEFAULT_DAYS=mon,wed,fri`
- `CALENDAR_DEFAULT_TIMES=09:00,14:00,19:00`
- `CALENDAR_TIMEZONE=UTC`
- `CALENDAR_TOPICS=motivation,productivity,finance,mindset,ai tools`

## Notes

- Output files are written to `output/`.
- Temporary render assets are written to `work/`.
- Rendering requires external APIs/credentials.
- EXE/APK generation requires platform-specific toolchains.
