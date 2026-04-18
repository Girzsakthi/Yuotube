using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SkyPlaneAR.Rendering
{
    /// <summary>
    /// URP ScriptableRenderPass that blits the sky mask debug overlay
    /// after transparent geometry is rendered.
    /// Registered by SkyMaskRendererFeature.
    /// </summary>
    public class SkyMaskRenderPass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle _cameraColorTarget;
        private static readonly int SkyMaskTexID = Shader.PropertyToID("_SkyMask");

        public void Setup(Material material, RTHandle colorTarget)
        {
            _material = material;
            _cameraColorTarget = colorTarget;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null) return;

            var cmd = CommandBufferPool.Get("SkyPlaneAR_SkyMaskDebug");
            using (new ProfilingScope(cmd, new ProfilingSampler("SkyPlaneAR.SkyMaskDebug")))
            {
                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            _cameraColorTarget = null;
        }
    }

    /// <summary>
    /// ScriptableRendererFeature that injects SkyMaskRenderPass into the URP renderer.
    /// Add this feature in your URP Renderer asset (Project Settings > Graphics > URP Asset).
    /// </summary>
    [Serializable]
    public class SkyMaskRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class SkyMaskFeatureSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            public Material skyMaskMaterial;
        }

        public SkyMaskFeatureSettings settings = new SkyMaskFeatureSettings();
        private SkyMaskRenderPass _pass;

        public override void Create()
        {
            _pass = new SkyMaskRenderPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.skyMaskMaterial == null) return;

            _pass.renderPassEvent = settings.renderPassEvent;
            renderer.EnqueuePass(_pass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            _pass.Setup(settings.skyMaskMaterial, renderer.cameraColorTargetHandle);
        }
    }
}
