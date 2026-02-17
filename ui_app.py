import json
import subprocess
import sys
from pathlib import Path

import streamlit as st

ROOT = Path(__file__).parent
OUTPUT_DIR = ROOT / "output"
OUTPUT_DIR.mkdir(exist_ok=True)

st.set_page_config(page_title="Faceless Studio", page_icon="🎬", layout="wide")
st.title("🎬 Faceless Studio")
st.caption("Advanced UI for video rendering + 100-channel scheduling")


def run_command(args: list[str]) -> tuple[int, str, str]:
    proc = subprocess.run(
        [sys.executable, "main.py", *args],
        cwd=ROOT,
        text=True,
        capture_output=True,
    )
    return proc.returncode, proc.stdout, proc.stderr


tab_render, tab_calendar, tab_custom = st.tabs(["Render Video", "Generate Calendar", "Custom Channels JSON"])

with tab_render:
    st.subheader("Video Pipeline")
    col_a, col_b = st.columns(2)
    with col_a:
        topic = st.text_input("Topic", "3 Stoic habits for better focus")
        out_video = st.text_input("Output file", "output/video.mp4")
    with col_b:
        video_size = st.selectbox(
            "Video size",
            ["1080x1920", "720x1280", "1080x1080", "1920x1080", "1280x720"],
            index=0,
        )
        fps = st.selectbox("FPS", [24, 30, 60], index=1)
        quality = st.selectbox("Quality", ["low", "medium", "high"], index=1)

    if st.button("Render Video", type="primary"):
        if not topic.strip():
            st.error("Topic is required.")
        else:
            with st.spinner("Rendering video. This can take a while..."):
                code, out, err = run_command(
                    [
                        "render",
                        "--topic",
                        topic,
                        "--out",
                        out_video,
                        "--video-size",
                        video_size,
                        "--fps",
                        str(fps),
                        "--quality",
                        quality,
                    ]
                )
            st.code(out or "(no stdout)", language="bash")
            if code == 0:
                st.success(f"Video render completed: {out_video}")
            else:
                st.error("Render failed")
                st.code(err or "(no stderr)", language="bash")

with tab_calendar:
    st.subheader("Scalable Calendar Generator")

    col1, col2, col3 = st.columns(3)
    with col1:
        channels = st.number_input("Channels", min_value=1, max_value=2000, value=100, step=1)
        weeks = st.number_input("Weeks", min_value=1, max_value=104, value=4, step=1)
    with col2:
        start_date = st.date_input("Start date")
        timezone = st.text_input("Timezone", "UTC")
    with col3:
        topics = st.text_input("Topic pool (comma-separated)", "motivation,productivity,finance,mindset,ai tools")
        out_csv = st.text_input("Output CSV", "output/calendar.csv")

    channels_file = st.text_input("Optional channels JSON path", "")
    template_out = st.text_input("Optional template output path", "output/channels.template.json")

    if st.button("Generate Calendar", type="primary"):
        args = [
            "calendar",
            "--channels",
            str(channels),
            "--weeks",
            str(weeks),
            "--start-date",
            start_date.isoformat(),
            "--timezone",
            timezone,
            "--topics",
            topics,
            "--out",
            out_csv,
        ]
        if channels_file.strip():
            args.extend(["--channels-file", channels_file.strip()])
        if template_out.strip():
            args.extend(["--template-out", template_out.strip()])

        with st.spinner("Generating calendar..."):
            code, out, err = run_command(args)

        st.code(out or "(no stdout)", language="bash")
        if code == 0:
            st.success("Calendar generated successfully")
            generated = ROOT / out_csv
            if generated.exists():
                st.download_button(
                    "Download calendar.csv",
                    data=generated.read_bytes(),
                    file_name=generated.name,
                    mime="text/csv",
                )
        else:
            st.error("Calendar generation failed")
            st.code(err or "(no stderr)", language="bash")

with tab_custom:
    st.subheader("Create custom per-channel JSON")
    st.write("Use this to build and save a custom channels file, then use it in the Calendar tab.")

    sample = [
        {
            "channel": "finance_daily",
            "posts_per_week": 5,
            "days": ["mon", "tue", "wed", "thu", "fri"],
            "time_slots": ["08:00", "12:00", "18:00"],
            "topics": ["budgeting", "investing", "side hustles"],
            "priority": 2,
        }
    ]

    text = st.text_area("channels.json content", value=json.dumps(sample, indent=2), height=320)
    save_path = st.text_input("Save path", "output/channels.custom.json")

    if st.button("Validate + Save JSON"):
        try:
            parsed = json.loads(text)
            if not isinstance(parsed, list):
                raise ValueError("Top-level JSON must be an array")
            target = ROOT / save_path
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(json.dumps(parsed, indent=2), encoding="utf-8")
            st.success(f"Saved: {save_path}")
        except Exception as exc:
            st.error(f"Invalid JSON: {exc}")
