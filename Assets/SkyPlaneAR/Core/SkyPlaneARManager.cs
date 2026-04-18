using System.Collections;
using UnityEngine;
using SkyPlaneAR.API;
using SkyPlaneAR.PlaneDetection;
using SkyPlaneAR.SkyDetection;
using SkyPlaneAR.CloudDetection;
using SkyPlaneAR.Tracking;
using SkyPlaneAR.Rendering;

namespace SkyPlaneAR.Core
{
    [DisallowMultipleComponent]
    public sealed class SkyPlaneARManager : MonoBehaviour
    {
        public static SkyPlaneARManager Instance { get; private set; }

        public SkyPlaneARSettings Settings { get; private set; }
        public bool IsInitialized { get; private set; }

        // Sub-system references — internal access for SDK classes.
        internal CameraFeedHandler CameraFeed { get; private set; }
        internal FrameProcessor FrameProc { get; private set; }
        internal PlaneDetectionManager PlaneDetectionMgr { get; private set; }
        internal SkyDetector SkyDetectorMgr { get; private set; }
        internal CloudClassifier CloudClassifierMgr { get; private set; }
        internal AnchorManager AnchorMgr { get; private set; }
        internal SkyMaskRenderer SkyMaskRendererMgr { get; private set; }
        internal AROverlayRenderer OverlayRendererMgr { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initializes the SDK. Safe to call multiple times; subsequent calls are ignored.
        /// </summary>
        public void Initialize(SkyPlaneARSettings settings = null)
        {
            if (IsInitialized) return;

            Settings = settings != null
                ? settings
                : ScriptableObject.CreateInstance<SkyPlaneARSettings>();

            Application.targetFrameRate = Settings.targetFrameRate;
            StartCoroutine(InitializeAsync());
        }

        private IEnumerator InitializeAsync()
        {
            Debug.Log("[SkyPlaneAR] Initializing SDK...");

            // 1. Camera feed
            CameraFeed = new CameraFeedHandler();
            var arCamera = Camera.main;
            CameraFeed.Initialize(arCamera, Settings);

            // 2. Texture pool for sky detection pipeline
            var texPool = new TexturePool(
                Settings.texturePoolSize,
                Settings.skyModelInputWidth,
                Settings.skyModelInputHeight);

            // 3. Plane detection
            PlaneDetectionMgr = new PlaneDetectionManager();
            PlaneDetectionMgr.Initialize(Settings);
            PlaneDetectionMgr.StartDetection();

            // 4. Sky detector — coroutine for model loading
            var skyDetectorGO = new GameObject("SkyDetector");
            skyDetectorGO.transform.SetParent(transform);
            SkyDetectorMgr = skyDetectorGO.AddComponent<SkyDetector>();
            yield return StartCoroutine(SkyDetectorMgr.Initialize(Settings, texPool));

            // 5. Cloud classifier
            CloudClassifierMgr = new CloudClassifier(Settings);
            SkyPlaneAREvents.OnSkyMaskReady += mask =>
                CloudClassifierMgr.Classify(mask, CameraFeed.CurrentFrame);

            // 6. Anchor manager
            AnchorMgr = new AnchorManager(Settings);

            // 7. Rendering
            var renderGO = new GameObject("SkyPlaneARRenderers");
            renderGO.transform.SetParent(transform);

            SkyMaskRendererMgr = renderGO.AddComponent<SkyMaskRenderer>();
            SkyMaskRendererMgr.Initialize(Settings);

            OverlayRendererMgr = renderGO.AddComponent<AROverlayRenderer>();
            OverlayRendererMgr.Initialize(Settings, SkyMaskRendererMgr);

            SkyPlaneAREvents.OnSkyMaskReady += mask =>
            {
                SkyMaskRendererMgr.UpdateSkyMask(mask);
                OverlayRendererMgr.UpdateMaskOnMaterials(mask);
            };

            // 8. Frame processor — drives the per-frame loop
            FrameProc = gameObject.AddComponent<FrameProcessor>();
            FrameProc.Configure(Settings, CameraFeed, SkyDetectorMgr, PlaneDetectionMgr);

            IsInitialized = true;
            Debug.Log("[SkyPlaneAR] SDK initialized successfully.");
            SkyPlaneAREvents.RaiseSDKInitialized();
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            PlaneDetectionMgr?.Shutdown();
            SkyDetectorMgr?.Dispose();
            AnchorMgr?.Dispose();
            CameraFeed?.Dispose();

            IsInitialized = false;
            Debug.Log("[SkyPlaneAR] SDK shut down.");
        }

        private void OnDestroy()
        {
            Shutdown();
            if (Instance == this)
                Instance = null;
        }
    }
}
