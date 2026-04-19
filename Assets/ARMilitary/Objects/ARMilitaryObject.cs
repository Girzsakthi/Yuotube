using ARMilitary.Data;
using UnityEngine;

namespace ARMilitary.Objects
{
    [DisallowMultipleComponent]
    public class ARMilitaryObject : MonoBehaviour
    {
        public ObjectType ObjectType { get; set; }
        public string CommandId { get; set; }

        private Vector3 _targetScale;
        private float _spawnTimer;
        private const float SpawnDuration = 0.6f;

        private void Start()
        {
            _targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (_spawnTimer < SpawnDuration)
            {
                _spawnTimer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, _spawnTimer / SpawnDuration);
                transform.localScale = Vector3.Lerp(Vector3.zero, _targetScale, t);
            }
        }
    }
}
