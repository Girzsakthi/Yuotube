using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace SkyPlaneAR.Core
{
    public sealed class TexturePool : IDisposable
    {
        private readonly ConcurrentQueue<RenderTexture> _rtPool = new ConcurrentQueue<RenderTexture>();
        private readonly ConcurrentQueue<Texture2D> _tex2dPool = new ConcurrentQueue<Texture2D>();
        private readonly int _width;
        private readonly int _height;
        private readonly RenderTextureFormat _rtFormat;
        private readonly TextureFormat _texFormat;
        private bool _disposed;

        public TexturePool(int count, int width, int height,
                           RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32,
                           TextureFormat texFormat = TextureFormat.R8)
        {
            _width = width;
            _height = height;
            _rtFormat = rtFormat;
            _texFormat = texFormat;

            for (int i = 0; i < count; i++)
            {
                _rtPool.Enqueue(CreateRT());
                _tex2dPool.Enqueue(CreateTex2D());
            }
        }

        public RenderTexture RentRenderTexture()
        {
            if (_rtPool.TryDequeue(out var rt))
                return rt;
            return CreateRT();
        }

        public void ReturnRenderTexture(RenderTexture rt)
        {
            if (rt != null)
                _rtPool.Enqueue(rt);
        }

        public Texture2D RentTexture2D()
        {
            if (_tex2dPool.TryDequeue(out var tex))
                return tex;
            return CreateTex2D();
        }

        public void ReturnTexture2D(Texture2D tex)
        {
            if (tex != null)
                _tex2dPool.Enqueue(tex);
        }

        private RenderTexture CreateRT()
        {
            var rt = new RenderTexture(_width, _height, 0, _rtFormat)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            rt.Create();
            return rt;
        }

        private Texture2D CreateTex2D()
        {
            return new Texture2D(_width, _height, _texFormat, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            while (_rtPool.TryDequeue(out var rt))
            {
                if (rt != null)
                    UnityEngine.Object.Destroy(rt);
            }
            while (_tex2dPool.TryDequeue(out var tex))
            {
                if (tex != null)
                    UnityEngine.Object.Destroy(tex);
            }
        }
    }
}
