using UnityEngine;

namespace ARMilitary.Objects
{
    public class JetAnimator : MonoBehaviour
    {
        [SerializeField] private float circleRadius  = 4f;
        [SerializeField] private float circleSpeed   = 0.4f;
        [SerializeField] private float bankAngle     = 25f;
        [SerializeField] private float altitude      = 3f;

        private float _angle;
        private Vector3 _origin;

        private void Start() => _origin = transform.position;

        private void Update()
        {
            _angle += circleSpeed * Time.deltaTime;

            float x = Mathf.Cos(_angle) * circleRadius;
            float z = Mathf.Sin(_angle) * circleRadius;
            transform.position = _origin + new Vector3(x, altitude, z);

            // Face direction of travel
            float dx = -Mathf.Sin(_angle);
            float dz =  Mathf.Cos(_angle);
            var dir = new Vector3(dx, 0, dz);
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 0, -bankAngle);
        }
    }
}
