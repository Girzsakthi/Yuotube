using System.Collections;
using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.API;

namespace SkyPlaneAR.CloudDetection
{
    /// <summary>
    /// Lightweight cloud classifier using brightness-variance analysis on the sky mask.
    /// High variance in sky-region brightness = clouds present.
    /// No secondary ML model required; uses the sky mask from SkyDetector.
    /// </summary>
    public class CloudClassifier
    {
        private readonly SkyPlaneARSettings _settings;

        public CloudDetectionResult LastResult { get; private set; }
        public bool IsEnabled { get; private set; }

        public CloudClassifier(SkyPlaneARSettings settings)
        {
            _settings = settings;
            IsEnabled = settings.enableCloudDetection;
        }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        /// <summary>
        /// Analyzes the current camera frame within sky regions defined by skyMask.
        /// Called after a new sky mask is available.
        /// </summary>
        public void Classify(Texture2D skyMask, RenderTexture cameraFrame)
        {
            if (!IsEnabled || skyMask == null) return;

            float variance = ComputeBrightnessVariance(skyMask, cameraFrame);

            // Normalize variance to [0,1] confidence range.
            // Empirically, variance > 0.05 indicates cloud texture presence.
            float normalized = Mathf.Clamp01(variance / 0.1f);
            bool hasClouds = normalized >= _settings.cloudConfidenceThreshold;

            var result = new CloudDetectionResult(hasClouds, normalized);
            LastResult = result;
            SkyPlaneAREvents.RaiseCloudResult(result);
        }

        private float ComputeBrightnessVariance(Texture2D skyMask, RenderTexture cameraFrame)
        {
            // Read sky mask pixels (CPU readback — runs infrequently via throttle).
            var maskPixels = skyMask.GetPixels32();
            int w = skyMask.width;
            int h = skyMask.height;

            // Blit camera frame to a CPU-readable texture.
            var readTex = new Texture2D(w, h, TextureFormat.RGB24, false);
            var prevRT = RenderTexture.active;
            RenderTexture.active = cameraFrame;
            readTex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            RenderTexture.active = prevRT;
            var framePixels = readTex.GetPixels32();
            Object.Destroy(readTex);

            // Compute mean brightness in sky region.
            double sum = 0;
            double sumSq = 0;
            int count = 0;
            for (int i = 0; i < maskPixels.Length; i++)
            {
                if (maskPixels[i].r < 128) continue;    // not sky
                float brightness = (framePixels[i].r + framePixels[i].g + framePixels[i].b) / (3f * 255f);
                sum += brightness;
                sumSq += brightness * brightness;
                count++;
            }

            if (count == 0) return 0f;

            double mean = sum / count;
            double variance = (sumSq / count) - (mean * mean);
            return (float)variance;
        }
    }
}
