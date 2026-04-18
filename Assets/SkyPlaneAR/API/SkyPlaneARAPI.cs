using System;
using System.Collections.Generic;
using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.PlaneDetection;
using SkyPlaneAR.Tracking;

namespace SkyPlaneAR.API
{
    /// <summary>
    /// Primary public API for the SkyPlaneAR SDK.
    /// All methods are safe to call from any MonoBehaviour or script.
    /// Subscribe to SkyPlaneAREvents before calling Initialize().
    /// </summary>
    public static class SkyPlaneARAPI
    {
        public const string Version = "1.0.0";

        /// <summary>
        /// Initializes the SDK. Creates SkyPlaneARManager if not present in the scene.
        /// Fires SkyPlaneAREvents.OnSDKInitialized when ready.
        /// </summary>
        public static void Initialize(SkyPlaneARSettings settings = null)
        {
            if (SkyPlaneARManager.Instance != null && SkyPlaneARManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[SkyPlaneAR] SDK already initialized.");
                return;
            }

            if (SkyPlaneARManager.Instance == null)
            {
                var go = new GameObject("SkyPlaneARManager");
                go.AddComponent<SkyPlaneARManager>();
            }

            SkyPlaneARManager.Instance.Initialize(settings);
        }

        /// <summary>
        /// Registers a callback invoked each time a new plane is detected.
        /// Equivalent to subscribing to SkyPlaneAREvents.OnPlaneDetected.
        /// </summary>
        public static void OnPlaneDetected(Action<PlaneData> callback)
        {
            if (callback != null)
                SkyPlaneAREvents.OnPlaneDetected += callback;
        }

        /// <summary>
        /// Registers a callback invoked each time a plane is updated.
        /// </summary>
        public static void OnPlaneUpdated(Action<PlaneData> callback)
        {
            if (callback != null)
                SkyPlaneAREvents.OnPlaneUpdated += callback;
        }

        /// <summary>
        /// Enables or disables real-time sky segmentation.
        /// When enabled, SkyPlaneAREvents.OnSkyMaskReady fires at the configured inference rate.
        /// </summary>
        public static void EnableSkyDetection(bool enable)
        {
            if (!EnsureInitialized(nameof(EnableSkyDetection))) return;
            SkyPlaneARManager.Instance.SkyDetectorMgr.EnableSkyDetection(enable);
        }

        /// <summary>
        /// Returns the most recently computed sky mask texture.
        /// The returned texture is owned by the SDK pool — do NOT destroy it.
        /// </summary>
        public static SkyPlaneARResult<Texture2D> GetSkyMask()
        {
            if (!EnsureInitialized(nameof(GetSkyMask)))
                return SkyPlaneARResult<Texture2D>.Fail("SDK not initialized.");

            var mask = SkyPlaneARManager.Instance.SkyDetectorMgr.CurrentSkyMask;
            return mask != null
                ? SkyPlaneARResult<Texture2D>.Ok(mask)
                : SkyPlaneARResult<Texture2D>.Fail("No sky mask available yet. Enable sky detection first.");
        }

        /// <summary>
        /// Places an AR anchor on the specified plane at worldPosition.
        /// Returns anchor data including a stable ID and world pose.
        /// </summary>
        public static SkyPlaneARResult<AnchorData> PlaceAnchor(PlaneData plane, Vector3 worldPosition)
        {
            if (!EnsureInitialized(nameof(PlaceAnchor)))
                return SkyPlaneARResult<AnchorData>.Fail("SDK not initialized.");

            var anchor = SkyPlaneARManager.Instance.AnchorMgr.PlaceAnchor(plane, worldPosition);
            return SkyPlaneARResult<AnchorData>.Ok(anchor);
        }

        /// <summary>
        /// Enables or disables cloud detection. Requires sky detection to be active.
        /// Results are delivered via SkyPlaneAREvents.OnCloudDetectionResult.
        /// </summary>
        public static void EnableCloudDetection(bool enable)
        {
            if (!EnsureInitialized(nameof(EnableCloudDetection))) return;
            SkyPlaneARManager.Instance.CloudClassifierMgr.SetEnabled(enable);
        }

        /// <summary>Returns all currently tracked planes.</summary>
        public static IReadOnlyDictionary<string, PlaneData> GetDetectedPlanes()
        {
            if (!EnsureInitialized(nameof(GetDetectedPlanes)))
                return new Dictionary<string, PlaneData>();

            return SkyPlaneARManager.Instance.PlaneDetectionMgr.DetectedPlanes;
        }

        /// <summary>
        /// Shuts down the SDK and releases all resources.
        /// Does NOT clear event subscriptions on SkyPlaneAREvents to avoid breaking user code.
        /// </summary>
        public static void Shutdown()
        {
            SkyPlaneARManager.Instance?.Shutdown();
        }

        private static bool EnsureInitialized(string callerName)
        {
            if (SkyPlaneARManager.Instance == null || !SkyPlaneARManager.Instance.IsInitialized)
            {
                Debug.LogError($"[SkyPlaneAR] {callerName}: SDK not initialized. Call SkyPlaneARAPI.Initialize() first.");
                return false;
            }
            return true;
        }
    }
}
