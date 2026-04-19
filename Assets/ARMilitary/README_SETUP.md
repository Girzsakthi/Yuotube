# AR Military Trainer — Setup Guide

## Requirements
- Unity 2022.3 LTS
- Android Build Support module
- Packages installed via manifest.json:
  - AR Foundation 5.1.0
  - ARCore XR Plugin 5.1.0
  - Universal Render Pipeline 14.0.9
  - XR Core Utils 2.3.0
  - XR Plugin Management 4.4.0

## Scene Setup (30 seconds)
1. Create a new **empty** scene in Unity
2. Delete the default **Main Camera** and **Directional Light**
3. Create an empty GameObject, name it `Bootstrap`
4. Add component: **ARMilitary > Bootstrap** (`ARMilitaryBootstrap`)
5. Save the scene

## Project Settings
| Setting | Value |
|---------|-------|
| Platform | Android |
| Min SDK | 24 (Android 7.0) |
| Target SDK | 34 |
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 (+ ARMv7 optional) |
| Graphics API | Vulkan (primary), OpenGLES3 (fallback) |
| XR Plugin Management → Android | ✅ ARCore |
| Graphics → Scriptable RP | Your URP Asset |

## URP Asset Setup
1. `Assets > Create > Rendering > URP Asset (with Universal Renderer)`
2. `Edit > Project Settings > Graphics`: assign URP Asset
3. `Edit > Project Settings > Quality`: assign URP Asset to all quality levels

## Build
```
File > Build Settings > Android > Build And Run
```
Both Instructor and Player devices must run the **same APK**.
Both must be on the **same WiFi network**.

## How to Use
1. Launch app on **Instructor** tablet → tap **INSTRUCTOR**
2. Launch app on **Player** phone → tap **PLAYER**
3. On Instructor: select objects (DRONE, JET, TANKER, BUNKER), tap **SEND**
4. On Player: AR objects spawn in the real world via plane detection

## Architecture
```
ARMilitaryBootstrap          ← single scene entry point
├── MainThreadDispatcher     ← thread-safe Unity main-thread dispatch
├── NetworkManager           ← owns UdpBroadcaster (port 7777 UDP broadcast)
├── ARSession + XROrigin     ← AR Foundation session & camera
├── PlaneAnchorManager       ← places objects on AR planes
│
├── [INSTRUCTOR mode]
│   ├── InstructorController ← selection state, send logic
│   └── InstructorUIManager  ← runtime-built grid UI
│
└── [PLAYER mode]
    ├── PlayerController     ← receives UDP spawns → PlaneAnchorManager
    └── MilitaryHUD          ← coordinates, WiFi signal, targeting reticle
```

## Supported Object Types
| Type | Category | Height Offset | Behaviour |
|------|----------|--------------|-----------|
| Drone | Air | +2m | Hovers, rotates slowly |
| Jet | Air | +5m | Circles the spawn point |
| Tanker | Ground | 0m | Static on plane |
| Bunker | Ground | 0m | Static on plane |
