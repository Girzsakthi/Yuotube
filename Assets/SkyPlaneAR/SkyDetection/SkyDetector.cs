using System;
using System.Collections;
using UnityEngine;
using SkyPlaneAR.Core;
using SkyPlaneAR.API;

#if SKYPLANEAR_BARRACUDA
using Unity.Barracuda;
#endif

namespace SkyPlaneAR.SkyDetection
{
    [DisallowMultipleComponent]
    public class SkyDetector : MonoBehaviour, IDisposable
    {
        public bool IsEnabled { get; private set; }
        public Texture2D CurrentSkyMask { get; private set; }

        private BarracudaInferenceRunner _inferenceRunner;
        private SkyMaskPostProcessor _postProcessor;
        private SkyPlaneARSettings _settings;
        private TexturePool _pool;
        private bool _inferenceRunning;
        private bool _disposed;

        public IEnumerator Initialize(SkyPlaneARSettings settings, TexturePool pool)
        {
            _settings = settings;
            _pool = pool;

            _inferenceRunner = new BarracudaInferenceRunner(settings);
            _postProcessor = new SkyMaskPostProcessor(settings.skyMaskThreshold, pool);

            yield return StartCoroutine(_inferenceRunner.LoadModelAsync(settings.skyModelRelativePath));

            if (!_inferenceRunner.IsModelLoaded)
                Debug.LogWarning("[SkyPlaneAR] Sky model not loaded. Sky detection will be unavailable.");

            if (settings.enableSkyDetectionOnStart)
                EnableSkyDetection(true);
        }

        public void EnableSkyDetection(bool enable)
        {
            IsEnabled = enable;
            Debug.Log($"[SkyPlaneAR] Sky detection {(enable ? "enabled" : "disabled")}.");
        }

        /// <summary>Called by FrameProcessor each throttled frame.</summary>
        public void Tick(RenderTexture cameraFrame)
        {
            if (!IsEnabled || _inferenceRunning || cameraFrame == null)
                return;

            if (!_inferenceRunner.IsModelLoaded)
            {
                // Emit a blank mask so the rendering pipeline still functions.
                EmitBlankMask();
                return;
            }

            StartCoroutine(RunDetectionPipeline(cameraFrame));
        }

        private IEnumerator RunDetectionPipeline(RenderTexture frame)
        {
            _inferenceRunning = true;

#if SKYPLANEAR_BARRACUDA
            Tensor outputTensor = null;
            _inferenceRunner.OnInferenceComplete += t => outputTensor = t;
            yield return StartCoroutine(_inferenceRunner.RunInferenceAsync(frame));
            _inferenceRunner.OnInferenceComplete -= t => outputTensor = t;

            if (outputTensor != null)
            {
                var previousMask = CurrentSkyMask;
                CurrentSkyMask = _postProcessor.Process(
                    outputTensor,
                    _settings.skyModelInputWidth,
                    _settings.skyModelInputHeight);

                // Return previous mask to pool.
                if (previousMask != null && previousMask != CurrentSkyMask)
                    _pool.ReturnTexture2D(previousMask);

                SkyPlaneAREvents.RaiseSkyMaskReady(CurrentSkyMask);
            }
#else
            yield return null;
            EmitBlankMask();
#endif
            _inferenceRunning = false;
        }

        private void EmitBlankMask()
        {
            if (CurrentSkyMask == null)
                CurrentSkyMask = _pool.RentTexture2D();
            SkyPlaneAREvents.RaiseSkyMaskReady(CurrentSkyMask);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _inferenceRunner?.Dispose();

            if (CurrentSkyMask != null)
                _pool?.ReturnTexture2D(CurrentSkyMask);
        }

        private void OnDestroy() => Dispose();
    }
}
