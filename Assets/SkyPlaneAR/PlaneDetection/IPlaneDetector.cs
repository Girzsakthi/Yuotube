using System;

namespace SkyPlaneAR.PlaneDetection
{
    public interface IPlaneDetector
    {
        bool IsSupported { get; }
        void StartDetection(PlaneDetectionConfig config);
        void StopDetection();

        /// <summary>Called every frame by FrameProcessor. Event-driven detectors can leave this empty.</summary>
        void Tick();

        event Action<PlaneData> OnPlaneAdded;
        event Action<PlaneData> OnPlaneUpdated;
        event Action<string> OnPlaneRemoved;
    }
}
