# Changelog

All notable changes to the SkyPlaneAR SDK will be documented in this file.
Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

---

## [1.0.0] - 2026-04-18

### Added

#### Core
- `SkyPlaneARManager`: DontDestroyOnLoad singleton, async initialization pipeline
- `CameraFeedHandler`: AR Foundation camera feed with fallback RenderTexture
- `FrameProcessor`: Per-frame dispatch with configurable inference throttle
- `TexturePool`: Pre-allocated `RenderTexture` + `Texture2D` pool (zero per-frame GC)
- `SkyPlaneARSettings`: ScriptableObject for all SDK configuration

#### Plane Detection
- `ARFoundationPlaneDetector`: Wraps `ARPlaneManager`, event-driven, horizontal + vertical
- `FallbackPlaneDetector`: Synthetic planes for Editor and non-AR devices
- `PlaneDetectionManager`: Auto-selects detector, re-evaluates every 5s for late AR init
- `PlaneData`: Immutable struct with center, normal, size, alignment, boundary polygon

#### Sky Detection
- `BarracudaInferenceRunner`: Async coroutine-based ONNX inference (no main thread stall)
- `SkyMaskPostProcessor`: `NativeArray<Color32>` bulk pixel write, threshold binarization
- `SkyDetector`: Orchestrator with enable/disable toggle and pooled mask management

#### Cloud Detection
- `CloudClassifier`: Brightness-variance analysis on sky mask region (no secondary model)
- `CloudDetectionResult`: HasClouds, Confidence, Timestamp

#### Tracking
- `AnchorManager`: AR Foundation anchor + transform fallback, per-frame pose update
- `WorldPositionStabilizer`: EMA smoothing for anchor jitter suppression
- `AnchorData`: Stable ID, world pose, attached GameObject

#### Rendering
- `SkyMask.shader`: URP fullscreen debug overlay, `step()` sky mask blend
- `AROverlay.shader`: URP PBR object shader with `clip()` sky-region discard
- `SkyMaskRenderPass` / `SkyMaskRendererFeature`: URP `ScriptableRendererFeature`
- `SkyMaskRenderer`: `MaterialPropertyBlock`-based mask upload
- `AROverlayRenderer`: Batch mask update for registered overlay objects

#### Public API
- `SkyPlaneARAPI`: Static facade — `Initialize`, `OnPlaneDetected`, `EnableSkyDetection`,
  `GetSkyMask`, `PlaceAnchor`, `EnableCloudDetection`, `GetDetectedPlanes`, `Shutdown`
- `SkyPlaneAREvents`: Action-based event bus, all events on main thread
- `SkyPlaneARResult<T>`: Discriminated union result type

#### Editor
- `SkyPlaneARSettingsEditor`: Custom inspector with foldout groups, performance estimate widget
- `SkyPlaneAREditor`: `Window > SkyPlaneAR` — dependency status, sample launcher, package export
- `OnnxModelImportPostProcessor`: Auto-validates `.onnx` assets in `SkyDetection/Models/`

#### Tests
- 15 NUnit tests covering core, plane detection, sky detection, and API surface validation

#### Samples
- `BasicARSample`: Plane detection → cube spawn on first horizontal plane
- `SkyOverlaySample`: Sky mask display + cloud detection + AROverlay object placement
