[app]
title = FacelessStudio
package.name = facelessstudio
package.domain = org.example
source.dir = .
source.include_exts = py,png,jpg,jpeg,json,csv,txt,env
version = 0.1.0
requirements = python3,requests
orientation = portrait
fullscreen = 0

# Entrypoint script
entrypoint = main.py

[buildozer]
log_level = 2
warn_on_root = 1

[app:android]
android.permissions = INTERNET
android.api = 31
android.minapi = 24
android.ndk = 25b
android.archs = arm64-v8a,armeabi-v7a
