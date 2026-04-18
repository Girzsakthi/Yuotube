using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SkyPlaneAR.Core;

namespace SkyPlaneAR.Tests
{
    [Category("SkyPlaneAR")]
    public class CoreTests
    {
        [Test]
        public void TexturePool_RentAndReturn_DoesNotAllocateAfterWarmup()
        {
            var pool = new TexturePool(4, 256, 256);
            try
            {
                // Warm up — rent and return.
                var rt1 = pool.RentRenderTexture();
                var rt2 = pool.RentRenderTexture();
                pool.ReturnRenderTexture(rt1);
                pool.ReturnRenderTexture(rt2);

                // Second rent should reuse pooled objects.
                long before = System.GC.GetTotalMemory(false);
                var rt3 = pool.RentRenderTexture();
                long after = System.GC.GetTotalMemory(false);
                pool.ReturnRenderTexture(rt3);

                // Allow small GC headroom but no large RenderTexture allocation.
                Assert.Less(after - before, 1024 * 100, "RenderTexture pool caused unexpected allocation.");
            }
            finally
            {
                pool.Dispose();
            }
        }

        [Test]
        public void TexturePool_Tex2D_RentAndReturn_Roundtrip()
        {
            var pool = new TexturePool(2, 64, 64);
            try
            {
                var tex = pool.RentTexture2D();
                Assert.IsNotNull(tex);
                Assert.AreEqual(64, tex.width);
                Assert.AreEqual(64, tex.height);
                pool.ReturnTexture2D(tex);

                // After return, renting again should give same texture back.
                var tex2 = pool.RentTexture2D();
                Assert.AreSame(tex, tex2);
                pool.ReturnTexture2D(tex2);
            }
            finally
            {
                pool.Dispose();
            }
        }

        [Test]
        public void SkyPlaneARSettings_Defaults_AreWithinValidRanges()
        {
            var settings = ScriptableObject.CreateInstance<SkyPlaneARSettings>();
            Assert.Greater(settings.targetFrameRate, 0);
            Assert.GreaterOrEqual(settings.inferenceThrottleFrames, 1);
            Assert.Greater(settings.texturePoolSize, 0);
            Assert.Greater(settings.skyModelInputWidth, 0);
            Assert.Greater(settings.skyModelInputHeight, 0);
            Assert.GreaterOrEqual(settings.skyMaskThreshold, 0f);
            Assert.LessOrEqual(settings.skyMaskThreshold, 1f);
            Assert.Greater(settings.minimumPlaneArea, 0f);
            Object.DestroyImmediate(settings);
        }

        [UnityTest]
        public IEnumerator SkyPlaneARManager_Instance_NullBeforeInitialize()
        {
            // Ensure no lingering instance from other tests.
            if (SkyPlaneARManager.Instance != null)
                Object.Destroy(SkyPlaneARManager.Instance.gameObject);

            yield return null;

            Assert.IsNull(SkyPlaneARManager.Instance,
                "SkyPlaneARManager.Instance should be null before Initialize() is called.");
        }
    }
}
