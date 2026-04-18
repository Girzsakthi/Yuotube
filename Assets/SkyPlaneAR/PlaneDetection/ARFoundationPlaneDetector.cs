using System;
using UnityEngine;

#if SKYPLANEAR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace SkyPlaneAR.PlaneDetection
{
#if SKYPLANEAR_ARFOUNDATION
    public class ARFoundationPlaneDetector : IPlaneDetector
    {
        private ARPlaneManager _arPlaneManager;
        private PlaneDetectionConfig _config;

        public bool IsSupported =>
            ARSession.state >= ARSessionState.SessionTracking;

        public event Action<PlaneData> OnPlaneAdded;
        public event Action<PlaneData> OnPlaneUpdated;
        public event Action<string> OnPlaneRemoved;

        public void StartDetection(PlaneDetectionConfig config)
        {
            _config = config;
            _arPlaneManager = UnityEngine.Object.FindObjectOfType<ARPlaneManager>();
            if (_arPlaneManager == null)
            {
                Debug.LogWarning("[SkyPlaneAR] ARPlaneManager not found in scene. Plane detection will not work.");
                return;
            }

            var detectionMode = PlaneDetectionMode.None;
            if (config.DetectHorizontal)
                detectionMode |= PlaneDetectionMode.Horizontal;
            if (config.DetectVertical)
                detectionMode |= PlaneDetectionMode.Vertical;

            _arPlaneManager.requestedDetectionMode = detectionMode;
            _arPlaneManager.planesChanged += HandlePlanesChanged;
            _arPlaneManager.enabled = true;
        }

        public void StopDetection()
        {
            if (_arPlaneManager != null)
            {
                _arPlaneManager.planesChanged -= HandlePlanesChanged;
                _arPlaneManager.enabled = false;
            }
        }

        // AR Foundation is event-driven; no polling needed.
        public void Tick() { }

        private void HandlePlanesChanged(ARPlanesChangedEventArgs args)
        {
            foreach (var plane in args.added)
            {
                var data = ConvertARPlane(plane);
                if (data.Area >= _config.MinimumArea)
                    OnPlaneAdded?.Invoke(data);
            }

            foreach (var plane in args.updated)
            {
                var data = ConvertARPlane(plane);
                if (data.Area >= _config.MinimumArea)
                    OnPlaneUpdated?.Invoke(data);
            }

            foreach (var plane in args.removed)
                OnPlaneRemoved?.Invoke(plane.trackableId.ToString());
        }

        private PlaneData ConvertARPlane(ARPlane arPlane)
        {
            var id = arPlane.trackableId.ToString();
            var center = arPlane.transform.position;
            var normal = arPlane.normal;
            var size = arPlane.size;
            var alignment = MapAlignment(arPlane.alignment);
            var pose = new Pose(center, arPlane.transform.rotation);

            var boundary2D = new Vector2[arPlane.boundary.Length];
            for (int i = 0; i < arPlane.boundary.Length; i++)
                boundary2D[i] = arPlane.boundary[i];

            return new PlaneData(id, center, normal, size, alignment, pose, boundary2D);
        }

        private PlaneAlignment MapAlignment(UnityEngine.XR.ARSubsystems.PlaneAlignment alignment)
        {
            switch (alignment)
            {
                case UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp:   return PlaneAlignment.HorizontalUp;
                case UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalDown: return PlaneAlignment.HorizontalDown;
                case UnityEngine.XR.ARSubsystems.PlaneAlignment.Vertical:       return PlaneAlignment.Vertical;
                default:                                                          return PlaneAlignment.NotAxisAligned;
            }
        }
    }
#endif
}
