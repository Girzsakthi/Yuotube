using UnityEngine;
using SkyPlaneAR.API;
using SkyPlaneAR.Core;
using SkyPlaneAR.PlaneDetection;
using SkyPlaneAR.Tracking;

namespace SkyPlaneAR.Samples
{
    /// <summary>
    /// Sample: detects horizontal planes and spawns a cube anchor on the first one.
    ///
    /// Scene Setup:
    ///   1. AR Session GO: ARSession + ARInputManager
    ///   2. AR Session Origin GO: ARSessionOrigin, ARCameraManager, ARPlaneManager
    ///   3. SkyPlaneARSettings ScriptableObject (Create > SkyPlaneAR > Settings)
    ///   4. This controller on any GO, with _settings and _anchorPrefab assigned
    ///   5. URP Renderer with SkyMaskRendererFeature added
    /// </summary>
    public class BasicARSampleController : MonoBehaviour
    {
        [SerializeField] private SkyPlaneARSettings _settings;
        [SerializeField] private GameObject _anchorPrefab;
        [SerializeField] private bool _spawnOnlyOnce = true;

        private bool _spawned;

        private void Start()
        {
            // Subscribe before Initialize so we don't miss early events.
            SkyPlaneAREvents.OnSDKInitialized += OnSDKReady;
            SkyPlaneAREvents.OnSDKError       += OnSDKError;
            SkyPlaneARAPI.OnPlaneDetected(OnPlaneDetected);

            SkyPlaneARAPI.Initialize(_settings);
        }

        private void OnSDKReady()
        {
            Debug.Log("[BasicARSample] SDK initialized. Waiting for planes...");
        }

        private void OnSDKError(string error)
        {
            Debug.LogError($"[BasicARSample] SDK error: {error}");
        }

        private void OnPlaneDetected(PlaneData plane)
        {
            if (_spawnOnlyOnce && _spawned) return;
            if (plane.Alignment != PlaneAlignment.HorizontalUp) return;

            var result = SkyPlaneARAPI.PlaceAnchor(plane, plane.Center);
            if (!result.Success)
            {
                Debug.LogWarning($"[BasicARSample] Failed to place anchor: {result.Error}");
                return;
            }

            AnchorData anchor = result.Value;

            if (_anchorPrefab != null)
            {
                var go = Instantiate(_anchorPrefab, anchor.WorldPose.position, anchor.WorldPose.rotation);
                SkyPlaneARManager.Instance?.AnchorMgr.AttachGameObject(anchor.Id, go);
                Debug.Log($"[BasicARSample] Cube spawned at {anchor.WorldPose.position}");
            }

            _spawned = true;
        }

        private void OnDestroy()
        {
            SkyPlaneAREvents.OnSDKInitialized -= OnSDKReady;
            SkyPlaneAREvents.OnSDKError       -= OnSDKError;
            SkyPlaneARAPI.Shutdown();
        }
    }
}
