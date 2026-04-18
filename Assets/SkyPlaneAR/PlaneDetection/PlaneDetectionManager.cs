using System;
using System.Collections.Generic;
using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.API;

#if SKYPLANEAR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

namespace SkyPlaneAR.PlaneDetection
{
    public class PlaneDetectionManager
    {
        private IPlaneDetector _activeDetector;
        private readonly Dictionary<string, PlaneData> _planes = new Dictionary<string, PlaneData>();
        private SkyPlaneARSettings _settings;
        private float _detectorReevalTimer;
        private const float ReevalInterval = 5f;

        public IReadOnlyDictionary<string, PlaneData> DetectedPlanes => _planes;
        public bool IsRunning { get; private set; }

        public void Initialize(SkyPlaneARSettings settings)
        {
            _settings = settings;
            SelectDetector();
        }

        public void StartDetection()
        {
            if (_activeDetector == null)
                SelectDetector();

            var config = new PlaneDetectionConfig(
                _settings.detectHorizontalPlanes,
                _settings.detectVerticalPlanes,
                _settings.minimumPlaneArea);

            _activeDetector.OnPlaneAdded += OnPlaneAdded;
            _activeDetector.OnPlaneUpdated += OnPlaneUpdated;
            _activeDetector.OnPlaneRemoved += OnPlaneRemoved;
            _activeDetector.StartDetection(config);
            IsRunning = true;
            Debug.Log($"[SkyPlaneAR] Plane detection started using {_activeDetector.GetType().Name}");
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            _activeDetector?.Tick();

            // Periodically re-evaluate detector in case AR Foundation initializes late.
            _detectorReevalTimer += deltaTime;
            if (_detectorReevalTimer >= ReevalInterval)
            {
                _detectorReevalTimer = 0f;
                TryUpgradeDetector();
            }
        }

        public void Shutdown()
        {
            if (_activeDetector != null)
            {
                _activeDetector.OnPlaneAdded -= OnPlaneAdded;
                _activeDetector.OnPlaneUpdated -= OnPlaneUpdated;
                _activeDetector.OnPlaneRemoved -= OnPlaneRemoved;
                _activeDetector.StopDetection();
            }
            _planes.Clear();
            IsRunning = false;
        }

        private void SelectDetector()
        {
#if SKYPLANEAR_ARFOUNDATION
            var arFoundationDetector = new ARFoundationPlaneDetector();
            if (arFoundationDetector.IsSupported)
            {
                _activeDetector = arFoundationDetector;
                return;
            }
#endif
            _activeDetector = new FallbackPlaneDetector();
            Debug.Log("[SkyPlaneAR] AR Foundation unavailable. Using FallbackPlaneDetector.");
        }

        private void TryUpgradeDetector()
        {
            if (_activeDetector is FallbackPlaneDetector)
            {
#if SKYPLANEAR_ARFOUNDATION
                var candidate = new ARFoundationPlaneDetector();
                if (candidate.IsSupported)
                {
                    Debug.Log("[SkyPlaneAR] AR Foundation now available. Upgrading plane detector.");
                    Shutdown();
                    _activeDetector = candidate;
                    StartDetection();
                }
#endif
            }
        }

        private void OnPlaneAdded(PlaneData data)
        {
            _planes[data.Id] = data;
            SkyPlaneAREvents.RaisePlaneDetected(data);
        }

        private void OnPlaneUpdated(PlaneData data)
        {
            _planes[data.Id] = data;
            SkyPlaneAREvents.RaisePlaneUpdated(data);
        }

        private void OnPlaneRemoved(string id)
        {
            _planes.Remove(id);
            SkyPlaneAREvents.RaisePlaneRemoved(id);
        }
    }
}
