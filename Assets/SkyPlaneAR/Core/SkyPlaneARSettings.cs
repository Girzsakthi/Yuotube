using UnityEngine;

namespace SkyPlaneAR.Core
{
    [CreateAssetMenu(fileName = "SkyPlaneARSettings", menuName = "SkyPlaneAR/Settings")]
    public class SkyPlaneARSettings : ScriptableObject
    {
        [Header("Performance")]
        [Tooltip("Target application frame rate.")]
        public int targetFrameRate = 30;

        [Tooltip("Run ML inference every N frames. Higher = better perf, lower = fresher masks.")]
        [Range(1, 10)]
        public int inferenceThrottleFrames = 3;

        [Tooltip("Number of pre-allocated RenderTexture and Texture2D objects in the texture pool.")]
        [Range(2, 8)]
        public int texturePoolSize = 4;

        [Header("Sky Detection")]
        public int skyModelInputWidth = 256;
        public int skyModelInputHeight = 256;

        [Range(0f, 1f)]
        public float skyMaskThreshold = 0.5f;

        public bool enableSkyDetectionOnStart = false;

        [Tooltip("Path relative to StreamingAssets for the sky segmentation ONNX model.")]
        public string skyModelRelativePath = "SkyPlaneAR/sky_segmentation.onnx";

        [Header("Plane Detection")]
        public bool detectHorizontalPlanes = true;
        public bool detectVerticalPlanes = true;

        [Tooltip("Ignore planes smaller than this area in square meters.")]
        public float minimumPlaneArea = 0.25f;

        [Header("Cloud Detection")]
        public bool enableCloudDetection = false;

        [Range(0f, 1f)]
        public float cloudConfidenceThreshold = 0.6f;

        [Header("Rendering")]
        public bool enableSkyMaskVisualization = false;
        public Color skyMaskDebugColor = new Color(0f, 0.5f, 1f, 0.4f);

        [Header("Tracking")]
        [Range(0.01f, 0.5f)]
        public float anchorPositionSmoothing = 0.1f;

        [Range(0.01f, 0.5f)]
        public float anchorRotationSmoothing = 0.1f;
    }
}
