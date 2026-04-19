using UnityEngine;

namespace ARMilitary.Objects
{
    public class DroneAnimator : MonoBehaviour
    {
        [SerializeField] private float hoverAmplitude = 0.08f;
        [SerializeField] private float hoverFrequency = 1.2f;
        [SerializeField] private float rotateSpeed    = 15f;

        private Vector3 _basePosition;

        private void Start() => _basePosition = transform.localPosition;

        private void Update()
        {
            float dy = Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            transform.localPosition = _basePosition + Vector3.up * dy;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public class RotorSpin : MonoBehaviour
    {
        private void Update() => transform.Rotate(Vector3.up, 1800f * Time.deltaTime, Space.Self);
    }
}
