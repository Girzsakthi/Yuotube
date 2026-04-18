# SkyPlaneAR SDK v1.0.0

Production-ready modular AR SDK for Unity 2022+/Unity 6 focused on:

- **Plane Detection** — horizontal & vertical surfaces via AR Foundation (with fallback for Editor/unsupported devices)
- **Sky Segmentation** — real-time binary sky mask via Unity Barracuda ML
- **Cloud Detection** — lightweight brightness-variance cloud classifier
- **Anchoring** — stable world anchors with EMA jitter smoothing
- **URP Rendering** — sky-mask-aware shaders, fullscreen debug overlay

---

## Quick Start

### 1. Install Dependencies (Package Manager)

```
com.unity.xr.arfoundation          5.1.0
com.unity.xr.arcore                5.1.0
com.unity.barracuda                3.0.0
com.unity.render-pipelines.universal 14.0.9
com.unity.xr.management            4.4.0
```

### 2. Create Settings Asset

`Right-click in Project > Create > SkyPlaneAR > Settings`

Assign the `SkyPlaneARSettings` asset to your controller's `_settings` field.

### 3. Minimal Integration

```csharp
using SkyPlaneAR.API;

void Start()
{
    // Subscribe to events BEFORE Initialize()
    SkyPlaneAREvents.OnPlaneDetected += plane =>
        Debug.Log($"Plane detected: {plane.Id} at {plane.Center}");

    SkyPlaneAREvents.OnSkyMaskReady += mask =>
        myRawImage.texture = mask;

    SkyPlaneARAPI.Initialize(mySettings);
    SkyPlaneARAPI.EnableSkyDetection(true);
}

void OnDestroy()
{
    SkyPlaneARAPI.Shutdown();
}
```

### 4. Full API Reference

| Method | Description |
|--------|-------------|
| `SkyPlaneARAPI.Initialize(settings)` | Start SDK. Creates SkyPlaneARManager. |
| `SkyPlaneARAPI.OnPlaneDetected(callback)` | Subscribe to plane detection. |
| `SkyPlaneARAPI.EnableSkyDetection(bool)` | Toggle ML sky segmentation. |
| `SkyPlaneARAPI.GetSkyMask()` | Get latest sky mask texture. |
| `SkyPlaneARAPI.PlaceAnchor(plane, pos)` | Place world anchor on a plane. |
| `SkyPlaneARAPI.EnableCloudDetection(bool)` | Toggle cloud classifier. |
| `SkyPlaneARAPI.GetDetectedPlanes()` | All currently tracked planes. |
| `SkyPlaneARAPI.Shutdown()` | Release all SDK resources. |

### 5. Scene Setup (Android AR)

1. **AR Session** GO: `ARSession` + `ARInputManager` components
2. **AR Session Origin** GO: `ARSessionOrigin`, `ARCameraManager`, `ARPlaneManager`
3. **URP Renderer Asset**: Add `SkyMaskRendererFeature` (found under `SkyPlaneAR.Rendering`)
4. Assign `skyMaskMaterial` (using `SkyPlaneAR/SkyMask` shader) to the renderer feature
5. **Project Settings > XR Plugin Management**: Enable ARCore (Android)
6. **Player Settings**: Minimum API Level 24+, Graphics API: Vulkan preferred

---

## Architecture

```
SkyPlaneARAPI (static facade)
    └── SkyPlaneARManager (MonoBehaviour, DontDestroyOnLoad)
            ├── CameraFeedHandler       — camera texture pipeline
            ├── FrameProcessor          — per-frame dispatch, throttle
            ├── PlaneDetectionManager   — IPlaneDetector adapter
            │       ├── ARFoundationPlaneDetector  (AR devices)
            │       └── FallbackPlaneDetector       (Editor/fallback)
            ├── SkyDetector             — Barracuda inference pipeline
            │       ├── BarracudaInferenceRunner
            │       └── SkyMaskPostProcessor
            ├── CloudClassifier         — brightness-variance classification
            ├── AnchorManager           — world anchor lifecycle
            ├── SkyMaskRenderer         — URP sky mask pass
            └── AROverlayRenderer       — mask-aware object rendering
```

---

## Performance

- **Target**: 30 FPS on mid-range Android (Snapdragon 700 series)
- **Inference**: Runs every 3 frames by default (~10 Hz at 30 FPS)
- **Textures**: Pre-allocated pool — no per-frame allocation
- **Barracuda backend**: `ComputePrecompiled` (GPU) on Vulkan/GLES3, `CSharpBurst` fallback

---

## Adding Your Sky Model

1. Export a segmentation model as ONNX: input `[1,3,256,256]`, output `[1,1,256,256]` sigmoid
2. Place the `.onnx` file in `Assets/StreamingAssets/SkyPlaneAR/`
3. Set `SkyPlaneARSettings.skyModelRelativePath` to match the filename
4. The `OnnxModelImportPostProcessor` will auto-validate on import

---

## Export as .unitypackage

Open `Window > SkyPlaneAR > Export SkyPlaneAR SDK as .unitypackage`

Or via CLI:
```bash
Unity -batchmode -nographics -quit \
  -projectPath /path/to/project \
  -executeMethod SkyPlaneAR.Editor.SkyPlaneAREditor.ExportUnityPackageCLI
```

---

## License

MIT License — see LICENSE file.
