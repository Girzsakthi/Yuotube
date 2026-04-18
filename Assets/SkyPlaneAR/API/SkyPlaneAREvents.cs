using System;
using UnityEngine;
using SkyPlaneAR.PlaneDetection;
using SkyPlaneAR.CloudDetection;

namespace SkyPlaneAR.API
{
    /// <summary>
    /// Central event bus for the SkyPlaneAR SDK.
    /// All events are guaranteed to be raised on the main Unity thread.
    /// Subscribe before calling SkyPlaneARAPI.Initialize().
    /// </summary>
    public static class SkyPlaneAREvents
    {
        public static event Action OnSDKInitialized;
        public static event Action<string> OnSDKError;
        public static event Action<PlaneData> OnPlaneDetected;
        public static event Action<PlaneData> OnPlaneUpdated;
        public static event Action<string> OnPlaneRemoved;
        public static event Action<Texture2D> OnSkyMaskReady;
        public static event Action<CloudDetectionResult> OnCloudDetectionResult;

        internal static void RaiseSDKInitialized() => OnSDKInitialized?.Invoke();
        internal static void RaiseSDKError(string msg) => OnSDKError?.Invoke(msg);
        internal static void RaisePlaneDetected(PlaneData data) => OnPlaneDetected?.Invoke(data);
        internal static void RaisePlaneUpdated(PlaneData data) => OnPlaneUpdated?.Invoke(data);
        internal static void RaisePlaneRemoved(string id) => OnPlaneRemoved?.Invoke(id);
        internal static void RaiseSkyMaskReady(Texture2D mask) => OnSkyMaskReady?.Invoke(mask);
        internal static void RaiseCloudResult(CloudDetectionResult result) => OnCloudDetectionResult?.Invoke(result);

        /// <summary>
        /// Clears all subscriptions. Called by SkyPlaneARAPI.Shutdown().
        /// </summary>
        internal static void ClearAll()
        {
            OnSDKInitialized = null;
            OnSDKError = null;
            OnPlaneDetected = null;
            OnPlaneUpdated = null;
            OnPlaneRemoved = null;
            OnSkyMaskReady = null;
            OnCloudDetectionResult = null;
        }
    }
}
