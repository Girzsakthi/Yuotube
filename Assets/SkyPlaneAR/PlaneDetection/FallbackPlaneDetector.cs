using System;
using UnityEngine;

namespace SkyPlaneAR.PlaneDetection
{
    /// <summary>
    /// Editor/non-AR fallback. Emits synthetic planes on the first tick,
    /// then polls touch/mouse input for anchor testing.
    /// Always available regardless of platform or AR support.
    /// </summary>
    public class FallbackPlaneDetector : IPlaneDetector
    {
        public bool IsSupported => true;

        public event Action<PlaneData> OnPlaneAdded;
        public event Action<PlaneData> OnPlaneUpdated;
        public event Action<string> OnPlaneRemoved;

        private bool _planesEmitted;
        private PlaneDetectionConfig _config;

        private const string GroundPlaneId = "fallback_ground_0";
        private const string WallPlaneId = "fallback_wall_0";

        public void StartDetection(PlaneDetectionConfig config)
        {
            _config = config;
            _planesEmitted = false;
        }

        public void StopDetection()
        {
            _planesEmitted = false;
        }

        public void Tick()
        {
            if (!_planesEmitted)
            {
                if (_config.DetectHorizontal)
                    EmitSyntheticGroundPlane();
                if (_config.DetectVertical)
                    EmitSyntheticWallPlane();
                _planesEmitted = true;
            }
        }

        private void EmitSyntheticGroundPlane()
        {
            var center = new Vector3(0f, -0.8f, 2f);
            var normal = Vector3.up;
            var size = new Vector2(2f, 2f);
            var pose = new Pose(center, Quaternion.LookRotation(Vector3.forward, normal));
            var boundary = new Vector2[]
            {
                new Vector2(-1f, -1f), new Vector2(1f, -1f),
                new Vector2(1f,  1f),  new Vector2(-1f, 1f)
            };

            var plane = new PlaneData(GroundPlaneId, center, normal, size,
                                      PlaneAlignment.HorizontalUp, pose, boundary);
            OnPlaneAdded?.Invoke(plane);
            Debug.Log($"[SkyPlaneAR][Fallback] Emitted ground plane at {center}");
        }

        private void EmitSyntheticWallPlane()
        {
            var center = new Vector3(0f, 0f, 3f);
            var normal = Vector3.back;
            var size = new Vector2(2f, 2f);
            var pose = new Pose(center, Quaternion.LookRotation(normal, Vector3.up));
            var boundary = new Vector2[]
            {
                new Vector2(-1f, -1f), new Vector2(1f, -1f),
                new Vector2(1f,  1f),  new Vector2(-1f, 1f)
            };

            var plane = new PlaneData(WallPlaneId, center, normal, size,
                                      PlaneAlignment.Vertical, pose, boundary);
            OnPlaneAdded?.Invoke(plane);
            Debug.Log($"[SkyPlaneAR][Fallback] Emitted wall plane at {center}");
        }
    }
}
