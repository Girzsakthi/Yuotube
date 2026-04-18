using System;
using System.Collections.Generic;
using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.PlaneDetection;

#if SKYPLANEAR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif

namespace SkyPlaneAR.Tracking
{
    public class AnchorManager : IDisposable
    {
        private readonly Dictionary<string, AnchorData> _anchors = new Dictionary<string, AnchorData>();
        private readonly WorldPositionStabilizer _stabilizer;
        private bool _disposed;

#if SKYPLANEAR_ARFOUNDATION
        private ARAnchorManager _arAnchorManager;
#endif

        public IReadOnlyDictionary<string, AnchorData> ActiveAnchors => _anchors;

        public AnchorManager(SkyPlaneARSettings settings)
        {
            _stabilizer = new WorldPositionStabilizer(
                settings.anchorPositionSmoothing,
                settings.anchorRotationSmoothing);

#if SKYPLANEAR_ARFOUNDATION
            _arAnchorManager = UnityEngine.Object.FindObjectOfType<ARAnchorManager>();
#endif
        }

        /// <summary>
        /// Places an anchor at worldPosition on the specified plane.
        /// </summary>
        public AnchorData PlaceAnchor(PlaneData plane, Vector3 worldPosition)
        {
            var pose = new Pose(worldPosition, Quaternion.LookRotation(
                Vector3.ProjectOnPlane(Camera.main?.transform.forward ?? Vector3.forward, plane.Normal),
                plane.Normal));

            string id = Guid.NewGuid().ToString("N");

#if SKYPLANEAR_ARFOUNDATION
            if (_arAnchorManager != null)
            {
                var arAnchor = CreateARFoundationAnchor(pose);
                if (arAnchor != null)
                    id = arAnchor.trackableId.ToString();
            }
#endif
            var anchorData = new AnchorData(id, pose, plane.Id);
            _anchors[id] = anchorData;
            Debug.Log($"[SkyPlaneAR] Anchor placed: {anchorData}");
            return anchorData;
        }

        /// <summary>Attaches a GameObject to the anchor; its transform follows the anchor pose.</summary>
        public void AttachGameObject(string anchorId, GameObject go)
        {
            if (!_anchors.TryGetValue(anchorId, out var anchor))
            {
                Debug.LogWarning($"[SkyPlaneAR] AttachGameObject: anchor {anchorId} not found.");
                return;
            }
            anchor.AttachedGameObject = go;
            go.transform.SetPositionAndRotation(anchor.WorldPose.position, anchor.WorldPose.rotation);
        }

        public void RemoveAnchor(string anchorId)
        {
            if (!_anchors.TryGetValue(anchorId, out var anchor)) return;

            if (anchor.AttachedGameObject != null)
                UnityEngine.Object.Destroy(anchor.AttachedGameObject);

            _anchors.Remove(anchorId);
        }

        /// <summary>Called per-frame to update poses and apply stabilization.</summary>
        public void Tick()
        {
            foreach (var pair in _anchors)
            {
                var anchor = pair.Value;
                if (anchor.AttachedGameObject == null) continue;

#if SKYPLANEAR_ARFOUNDATION
                // Let AR Foundation drive the pose directly when available.
                var arAnchor = anchor.AttachedGameObject.GetComponent<ARAnchor>();
                if (arAnchor != null)
                {
                    var rawPose = new Pose(arAnchor.transform.position, arAnchor.transform.rotation);
                    anchor.WorldPose = _stabilizer.Stabilize(anchor.WorldPose, rawPose);
                    anchor.IsTracking = arAnchor.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.None;
                }
                else
#endif
                {
                    // Fallback: pose is fixed; just keep the GO in sync.
                    anchor.AttachedGameObject.transform.SetPositionAndRotation(
                        anchor.WorldPose.position, anchor.WorldPose.rotation);
                }
            }
        }

#if SKYPLANEAR_ARFOUNDATION
        private ARAnchor CreateARFoundationAnchor(Pose pose)
        {
            if (_arAnchorManager == null) return null;
            var go = new GameObject("SkyPlaneARAnchor");
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            return go.AddComponent<ARAnchor>();
        }
#endif

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _anchors.Clear();
        }
    }
}
