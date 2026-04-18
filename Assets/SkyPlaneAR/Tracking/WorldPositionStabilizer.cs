using UnityEngine;

namespace SkyPlaneAR.Tracking
{
    /// <summary>
    /// Applies exponential moving average (EMA) smoothing to anchor poses
    /// to suppress jitter from AR tracking noise.
    /// </summary>
    public class WorldPositionStabilizer
    {
        private readonly float _posAlpha;
        private readonly float _rotAlpha;

        /// <param name="posAlpha">Position smoothing factor [0,1]. Lower = smoother but more lag.</param>
        /// <param name="rotAlpha">Rotation smoothing factor [0,1].</param>
        public WorldPositionStabilizer(float posAlpha = 0.1f, float rotAlpha = 0.1f)
        {
            _posAlpha = Mathf.Clamp01(posAlpha);
            _rotAlpha = Mathf.Clamp01(rotAlpha);
        }

        public Pose Stabilize(Pose current, Pose target)
        {
            var smoothedPos = Vector3.Lerp(current.position, target.position, _posAlpha);
            var smoothedRot = Quaternion.Slerp(current.rotation, target.rotation, _rotAlpha);
            return new Pose(smoothedPos, smoothedRot);
        }
    }
}
