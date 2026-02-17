# Faceless Studio (Simple Guide)

This app helps you:
- Generate faceless videos with AI
- Create posting calendars for many channels
- Use a simple UI instead of only CLI

---

## From scratch (quick start)

### 1) Open terminal in project folder

```bash
cd /workspace/Yuotube
```

### 2) One-command setup (recommended)

```bash
bash scripts/setup_from_scratch.sh
```

Or do it manually:

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
```

### 3) Add API keys

Now edit `.env` and set your keys:
- `OPENAI_API_KEY=...`
- `PEXELS_API_KEY=...` (needed only if `VISUAL_MODE=video`)

### 4) Start the UI

```bash
bash scripts/run_ui.sh
```

Open in browser:
- http://localhost:8501

---

## If you only want calendar (no video)

```bash
python main.py calendar --channels 100 --weeks 4 --start-date 2026-02-01 --out output/calendar.csv
```

---

## If you want one video from CLI

```bash
python main.py render --topic "3 Stoic habits for better focus" --out output/video.mp4
```

---

## Build files

### Windows EXE

```bash
bash scripts/build_exe.sh
```

### Android APK

```bash
bash scripts/build_apk.sh
```

---

## Common errors

- **`streamlit is not installed`**
  - Run: `pip install -r requirements.txt`

- **Missing API key error**
  - Add keys in `.env`

- **APK build fails**
  - Install Android SDK + NDK + Java first

