using System;
using UnityEngine;
using SkyPlaneAR.Core;

#if SKYPLANEAR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

namespace SkyPlaneAR.Core
{
    public class CameraFeedHandler : IDisposable
    {
        public RenderTexture CurrentFrame { get; private set; }
        public bool IsReady { get; private set; }
        public event Action<RenderTexture> OnFrameAvailable;

        private Camera _arCamera;
        private RenderTexture _fallbackRT;
        private bool _usingARFoundation;
        private bool _disposed;

#if SKYPLANEAR_ARFOUNDATION
        private ARCameraManager _arCameraManager;
#endif

        public void Initialize(Camera arCamera, SkyPlaneARSettings settings)
        {
            _arCamera = arCamera;

#if SKYPLANEAR_ARFOUNDATION
            _arCameraManager = UnityEngine.Object.FindObjectOfType<ARCameraManager>();
            if (_arCameraManager != null)
            {
                _arCameraManager.frameReceived += OnARCameraFrameReceived;
                _usingARFoundation = true;
                Debug.Log("[SkyPlaneAR] CameraFeedHandler: using ARCameraManager.");
                return;
            }
#endif
            SetupFallbackCameraTexture(arCamera, settings);
        }

        /// <summary>Called each frame by FrameProcessor when not using AR Foundation events.</summary>
        public void Update()
        {
            if (_usingARFoundation) return;

            if (_fallbackRT != null && _arCamera != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = _fallbackRT;
                RenderTexture.active = prev;

                CurrentFrame = _fallbackRT;
                IsReady = true;
                OnFrameAvailable?.Invoke(CurrentFrame);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

#if SKYPLANEAR_ARFOUNDATION
            if (_arCameraManager != null)
                _arCameraManager.frameReceived -= OnARCameraFrameReceived;
#endif
            if (_arCamera != null && _arCamera.targetTexture == _fallbackRT)
                _arCamera.targetTexture = null;

            if (_fallbackRT != null)
                UnityEngine.Object.Destroy(_fallbackRT);
        }

#if SKYPLANEAR_ARFOUNDATION
        private void OnARCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            if (_arCamera == null) return;

            if (_arCamera.targetTexture == null)
                SetupFallbackCameraTexture(_arCamera, null);

            CurrentFrame = _arCamera.targetTexture;
            IsReady = true;
            OnFrameAvailable?.Invoke(CurrentFrame);
        }
#endif

        private void SetupFallbackCameraTexture(Camera cam, SkyPlaneARSettings settings)
        {
            int w = Screen.width;
            int h = Screen.height;

            _fallbackRT = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            _fallbackRT.Create();

            if (cam != null)
                cam.targetTexture = _fallbackRT;

            CurrentFrame = _fallbackRT;
            IsReady = true;
            Debug.Log("[SkyPlaneAR] CameraFeedHandler: using fallback RenderTexture.");
        }
    }
}
