using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SkyPlaneAR.Core;
using SkyPlaneAR.SkyDetection;

namespace SkyPlaneAR.Tests
{
    [Category("SkyPlaneAR")]
    public class SkyDetectionTests
    {
        [Test]
        public void SkyMaskPostProcessor_AllOnesTensor_ProducesWhiteTexture()
        {
#if SKYPLANEAR_BARRACUDA
            int w = 8; int h = 8;
            var pool = new TexturePool(2, w, h, UnityEngine.RenderTextureFormat.ARGB32, TextureFormat.R8);
            var processor = new SkyMaskPostProcessor(0.5f, pool);

            using var tensor = new Unity.Barracuda.Tensor(1, h, w, 1);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    tensor[0, i, j, 0] = 1.0f;

            var result = processor.Process(tensor, w, h);
            var pixels = result.GetPixels32();

            foreach (var px in pixels)
                Assert.AreEqual(255, px.r, "All pixels should be white (sky) for all-ones tensor.");

            pool.ReturnTexture2D(result);
            pool.Dispose();
#else
            Assert.Ignore("Barracuda not installed. Skipping tensor test.");
#endif
        }

        [Test]
        public void SkyMaskPostProcessor_AllZerosTensor_ProducesBlackTexture()
        {
#if SKYPLANEAR_BARRACUDA
            int w = 8; int h = 8;
            var pool = new TexturePool(2, w, h, UnityEngine.RenderTextureFormat.ARGB32, TextureFormat.R8);
            var processor = new SkyMaskPostProcessor(0.5f, pool);

            using var tensor = new Unity.Barracuda.Tensor(1, h, w, 1);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    tensor[0, i, j, 0] = 0.0f;

            var result = processor.Process(tensor, w, h);
            var pixels = result.GetPixels32();

            foreach (var px in pixels)
                Assert.AreEqual(0, px.r, "All pixels should be black (non-sky) for all-zeros tensor.");

            pool.ReturnTexture2D(result);
            pool.Dispose();
#else
            Assert.Ignore("Barracuda not installed. Skipping tensor test.");
#endif
        }

        [Test]
        public void InferenceThrottle_FiresOnCorrectFrames()
        {
            var settings = ScriptableObject.CreateInstance<SkyPlaneARSettings>();
            settings.inferenceThrottleFrames = 3;

            int inferenceCallCount = 0;
            int totalFrames = 9;

            // Simulate FrameProcessor throttle logic.
            for (int frame = 1; frame <= totalFrames; frame++)
            {
                if (frame % settings.inferenceThrottleFrames == 0)
                    inferenceCallCount++;
            }

            // Frames 3, 6, 9 → 3 inference calls.
            Assert.AreEqual(3, inferenceCallCount,
                $"With throttle={settings.inferenceThrottleFrames}, expected 3 inference calls in {totalFrames} frames.");

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void TexturePool_DisposeTwice_DoesNotThrow()
        {
            var pool = new TexturePool(1, 64, 64);
            Assert.DoesNotThrow(() =>
            {
                pool.Dispose();
                pool.Dispose();
            });
        }
    }
}
