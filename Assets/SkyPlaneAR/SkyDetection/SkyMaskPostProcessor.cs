using System;
using Unity.Collections;
using UnityEngine;
using SkyPlaneAR.Core;

#if SKYPLANEAR_BARRACUDA
using Unity.Barracuda;
#endif

namespace SkyPlaneAR.SkyDetection
{
    public class SkyMaskPostProcessor
    {
        private readonly float _threshold;
        private readonly TexturePool _pool;

        public SkyMaskPostProcessor(float threshold, TexturePool pool)
        {
            _threshold = threshold;
            _pool = pool;
        }

#if SKYPLANEAR_BARRACUDA
        /// <summary>
        /// Converts the Barracuda output tensor to a binary R8 Texture2D from the pool.
        /// The caller must return the texture to the pool when done.
        /// </summary>
        public Texture2D Process(Tensor outputTensor, int width, int height)
        {
            var tex = _pool.RentTexture2D();

            int pixelCount = width * height;
            var pixels = new NativeArray<Color32>(pixelCount, Allocator.Temp,
                                                  NativeArrayOptions.UninitializedMemory);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float val = outputTensor[0, y, x, 0];
                        byte mask = val >= _threshold ? (byte)255 : (byte)0;
                        pixels[y * width + x] = new Color32(mask, mask, mask, 255);
                    }
                }

                tex.SetPixelData(pixels, 0);
                tex.Apply(false, false);
            }
            finally
            {
                pixels.Dispose();
            }

            return tex;
        }
#else
        // Returns a blank white texture when Barracuda is unavailable.
        public Texture2D Process(object outputTensor, int width, int height)
        {
            var tex = _pool.RentTexture2D();
            return tex;
        }
#endif
    }
}
