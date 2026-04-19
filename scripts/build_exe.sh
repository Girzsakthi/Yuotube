#!/usr/bin/env bash
set -euo pipefail

APP_NAME="FacelessStudio"
ENTRYPOINT="main.py"
DIST_DIR="dist"
BUILD_DIR="build"

python -m pip install --upgrade pip
python -m pip install pyinstaller

pyinstaller \
  --noconfirm \
  --clean \
  --name "$APP_NAME" \
  --onefile \
  "$ENTRYPOINT"

echo "EXE build artifacts in $DIST_DIR/ (on Windows this will be ${APP_NAME}.exe)."
