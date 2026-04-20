using System.Collections.Generic;
using ARMilitary.Data;
using ARMilitary.Objects;
using ARMilitary.Primitives;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARMilitary.AR
{
    public class PlaneAnchorManager : MonoBehaviour
    {
        public static PlaneAnchorManager Instance { get; private set; }

        private ARRaycastManager _raycastManager;
        private Camera           _arCamera;

        private readonly Dictionary<string, GameObject> _spawnedObjects = new Dictionary<string, GameObject>();
        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(ARRaycastManager raycastMgr, Camera arCamera)
        {
            _raycastManager = raycastMgr;
            _arCamera       = arCamera;
        }

        public void PlaceObject(SpawnPayload payload)
        {
            if (_spawnedObjects.ContainsKey(payload.commandId)) return;

            if (!System.Enum.TryParse<ObjectType>(payload.objectType, true, out var objType))
            {
                Debug.LogWarning("[ARMilitary] Unknown object type: " + payload.objectType);
                return;
            }

            var spawnPos = GetSpawnPosition(payload);
            var anchor = CreateAnchor(spawnPos);
            var parent = anchor != null ? anchor.transform : CreateWorldTransform(spawnPos);

            var go = PrimitiveFactory.Build(objType, parent);
            go.GetComponent<ARMilitaryObject>().CommandId = payload.commandId;
            _spawnedObjects[payload.commandId] = go;

            Debug.Log($"[ARMilitary] Spawned {objType} at {spawnPos}");
        }

        public void RemoveObject(string commandId)
        {
            if (_spawnedObjects.TryGetValue(commandId, out var go))
            {
                ExplosionEffect.Spawn(go.transform.position);
                Destroy(go.transform.parent != null ? go.transform.parent.gameObject : go);
                _spawnedObjects.Remove(commandId);
            }
        }

        public void ClearAll()
        {
            foreach (var kv in _spawnedObjects)
            {
                var root = kv.Value.transform.parent != null ? kv.Value.transform.parent.gameObject : kv.Value;
                ExplosionEffect.Spawn(root.transform.position);
                Destroy(root);
            }
            _spawnedObjects.Clear();
        }

        public int SpawnedCount => _spawnedObjects.Count;

        private Vector3 GetSpawnPosition(SpawnPayload payload)
        {
            if (_arCamera == null) return Vector3.forward * payload.spawnDistance;

            // Try AR raycast to plane
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (_raycastManager != null &&
                _raycastManager.Raycast(screenCenter, _hits, TrackableType.PlaneWithinBounds) &&
                _hits.Count > 0)
            {
                var hitPose = _hits[0].pose;
                var dir = (_arCamera.transform.position - hitPose.position).normalized;
                dir.y = 0;
                if (dir == Vector3.zero) dir = _arCamera.transform.forward;
                return hitPose.position +
                       dir * payload.spawnDistance * -1f +
                       Vector3.up * payload.heightOffset;
            }

            // Fallback: place directly in front of camera
            var camFwd = _arCamera.transform.forward;
            camFwd.y = 0;
            if (camFwd == Vector3.zero) camFwd = Vector3.forward;
            return _arCamera.transform.position +
                   camFwd.normalized * payload.spawnDistance +
                   Vector3.up * payload.heightOffset;
        }

        // ARF 5.x removed manual ARAnchor instantiation.
        // We attach objects to trackable planes via pose directly;
        // world-space roots give equivalent stability for training use.
        private static ARAnchor CreateAnchor(Vector3 worldPos) => null;

        private static Transform CreateWorldTransform(Vector3 worldPos)
        {
            var go = new GameObject("ARMilitaryRoot");
            go.transform.position = worldPos;
            return go.transform;
        }
    }
}
