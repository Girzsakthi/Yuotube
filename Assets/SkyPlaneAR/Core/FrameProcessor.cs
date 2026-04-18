using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.SkyDetection;
using SkyPlaneAR.PlaneDetection;

namespace SkyPlaneAR.Core
{
    [DisallowMultipleComponent]
    public class FrameProcessor : MonoBehaviour
    {
        private SkyPlaneARSettings _settings;
        private CameraFeedHandler _cameraFeed;
        private SkyDetector _skyDetector;
        private PlaneDetectionManager _planeDetectionManager;
        private int _frameCount;

        public void Configure(SkyPlaneARSettings settings,
                              CameraFeedHandler feed,
                              SkyDetector skyDetector,
                              PlaneDetectionManager planeMgr)
        {
            _settings = settings;
            _cameraFeed = feed;
            _skyDetector = skyDetector;
            _planeDetectionManager = planeMgr;
        }

        private void Update()
        {
            _frameCount++;

            _cameraFeed?.Update();
            _planeDetectionManager?.Tick(Time.deltaTime);

            if (_skyDetector != null && _cameraFeed != null &&
                _cameraFeed.IsReady && ShouldRunInference())
            {
                _skyDetector.Tick(_cameraFeed.CurrentFrame);
            }
        }

        private bool ShouldRunInference()
        {
            return _settings != null &&
                   _frameCount % _settings.inferenceThrottleFrames == 0;
        }
    }
}
