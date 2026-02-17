#!/usr/bin/env bash
set -euo pipefail

if ! command -v streamlit >/dev/null 2>&1; then
  echo "streamlit is not installed. Install dependencies first:"
  echo "  python -m pip install -r requirements.txt"
  exit 1
fi

exec streamlit run ui_app.py --server.port 8501 --server.address 0.0.0.0
