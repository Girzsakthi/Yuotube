using UnityEngine;
using UnityEngine.UI;
using SkyPlaneAR.API;
using SkyPlaneAR.Core;
using SkyPlaneAR.PlaneDetection;
using SkyPlaneAR.CloudDetection;

namespace SkyPlaneAR.Samples
{
    /// <summary>
    /// Sample: enables sky detection and renders the sky mask as a UI overlay.
    /// Also places an AROverlay-shaded object on the first plane found.
    ///
    /// Scene Setup:
    ///   1. AR Session + AR Session Origin (same as BasicARSample)
    ///   2. Canvas with a RawImage named "SkyMaskDisplay" (fullscreen)
    ///   3. An "OverlayObject" GO with SkyPlaneAR/AROverlay material assigned
    ///   4. This controller on any GO with all fields assigned
    /// </summary>
    public class SkyOverlaySampleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkyPlaneARSettings _settings;
        [SerializeField] private RawImage _skyMaskDisplay;
        [SerializeField] private GameObject _overlayObjectPrefab;

        [Header("Debug")]
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _cloudStatusText;

        private bool _planePlaced;

        private void Start()
        {
            if (_settings != null)
            {
                _settings.enableSkyDetectionOnStart = true;
                _settings.enableSkyMaskVisualization = true;
                _settings.enableCloudDetection = true;
            }

            SkyPlaneAREvents.OnSDKInitialized    += OnSDKReady;
            SkyPlaneAREvents.OnSkyMaskReady      += OnSkyMaskReady;
            SkyPlaneAREvents.OnCloudDetectionResult += OnCloudResult;
            SkyPlaneARAPI.OnPlaneDetected(OnPlaneDetected);

            SkyPlaneARAPI.Initialize(_settings);
        }

        private void OnSDKReady()
        {
            SetStatus("SDK ready. Sky detection active.");
            SkyPlaneARAPI.EnableSkyDetection(true);
            SkyPlaneARAPI.EnableCloudDetection(true);
        }

        private void OnSkyMaskReady(Texture2D mask)
        {
            if (_skyMaskDisplay != null)
                _skyMaskDisplay.texture = mask;
        }

        private void OnCloudResult(CloudDetectionResult result)
        {
            if (_cloudStatusText != null)
                _cloudStatusText.text = result.HasClouds
                    ? $"Clouds detected ({result.Confidence:P0})"
                    : "Clear sky";
        }

        private void OnPlaneDetected(PlaneData plane)
        {
            if (_planePlaced || _overlayObjectPrefab == null) return;
            if (plane.Alignment != PlaneAlignment.HorizontalUp) return;

            var result = SkyPlaneARAPI.PlaceAnchor(plane, plane.Center);
            if (!result.Success) return;

            var go = Instantiate(_overlayObjectPrefab,
                                  result.Value.WorldPose.position,
                                  result.Value.WorldPose.rotation);

            // Register with overlay renderer so sky mask is applied to its material.
            var overlayRenderer = SkyPlaneARManager.Instance?.OverlayRendererMgr;
            var mat = go.GetComponentInChildren<Renderer>()?.sharedMaterial;
            if (overlayRenderer != null && mat != null)
                overlayRenderer.RegisterOverlayObject(go, mat);

            _planePlaced = true;
            SetStatus("Overlay object placed on plane.");
        }

        private void OnGUI()
        {
            // Runtime debug info at top-left.
            var planes = SkyPlaneARAPI.GetDetectedPlanes();
            GUI.Label(new Rect(10, 10, 300, 20), $"Planes: {planes?.Count ?? 0}  |  SkyPlaneAR v{SkyPlaneARAPI.Version}");
        }

        private void SetStatus(string msg)
        {
            Debug.Log($"[SkyOverlaySample] {msg}");
            if (_statusText != null) _statusText.text = msg;
        }

        private void OnDestroy()
        {
            SkyPlaneAREvents.OnSDKInitialized    -= OnSDKReady;
            SkyPlaneAREvents.OnSkyMaskReady      -= OnSkyMaskReady;
            SkyPlaneAREvents.OnCloudDetectionResult -= OnCloudResult;
            SkyPlaneARAPI.Shutdown();
        }
    }
}
