#!/usr/bin/env bash
set -euo pipefail

python -m venv .venv
source .venv/bin/activate
python -m pip install --upgrade pip
python -m pip install -r requirements.txt

if [[ ! -f .env ]]; then
  cp .env.example .env
  echo "Created .env from .env.example"
fi

echo "Setup complete."
echo "Next: edit .env and set OPENAI_API_KEY (and PEXELS_API_KEY if needed)."
echo "Then run: bash scripts/run_ui.sh"
