using System;

namespace SkyPlaneAR.CloudDetection
{
    public readonly struct CloudDetectionResult
    {
        public readonly bool HasClouds;
        public readonly float Confidence;
        public readonly DateTime Timestamp;

        public CloudDetectionResult(bool hasClouds, float confidence)
        {
            HasClouds = hasClouds;
            Confidence = confidence;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString() =>
            $"Cloud={HasClouds} Confidence={Confidence:F2} @ {Timestamp:HH:mm:ss}";
    }
}
