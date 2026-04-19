#!/usr/bin/env bash
set -euo pipefail

# Linux host helper for Android APK build using Buildozer.
# Requires Android SDK/NDK + Java + system build deps.

python -m pip install --upgrade pip
python -m pip install buildozer cython

if [[ ! -f buildozer.spec ]]; then
  echo "Missing buildozer.spec in repository root"
  exit 1
fi

buildozer -v android debug

echo "APK generated under bin/*.apk"
