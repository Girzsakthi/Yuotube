using System;
using System.Collections;
using System.IO;
using UnityEngine;
using SkyPlaneAR.Core;

#if SKYPLANEAR_BARRACUDA
using Unity.Barracuda;
#endif

namespace SkyPlaneAR.SkyDetection
{
#if SKYPLANEAR_BARRACUDA
    public class BarracudaInferenceRunner : IDisposable
    {
        private Model _runtimeModel;
        private IWorker _worker;
        private readonly SkyPlaneARSettings _settings;
        private bool _disposed;

        public bool IsModelLoaded { get; private set; }
        public event Action<Tensor> OnInferenceComplete;

        private static readonly string InputLayerName = "input";
        private static readonly string OutputLayerName = "output";

        public BarracudaInferenceRunner(SkyPlaneARSettings settings)
        {
            _settings = settings;
        }

        public IEnumerator LoadModelAsync(string streamingAssetsRelativePath)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsRelativePath);

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android StreamingAssets are inside the APK; use UnityWebRequest to extract.
            var req = UnityEngine.Networking.UnityWebRequest.Get(fullPath);
            yield return req.SendWebRequest();

            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SkyPlaneAR] Failed to load model from {fullPath}: {req.error}");
                yield break;
            }

            byte[] modelBytes = req.downloadHandler.data;
            _runtimeModel = ModelLoader.Load(new NNModel { modelData = new NNModelData { Value = modelBytes } });
#else
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[SkyPlaneAR] Model file not found at {fullPath}. Sky detection will use placeholder mode.");
                IsModelLoaded = false;
                yield break;
            }

            _runtimeModel = ModelLoader.LoadFromStreamingAssets(streamingAssetsRelativePath);
#endif
            if (_runtimeModel == null)
            {
                Debug.LogError("[SkyPlaneAR] Failed to load Barracuda model.");
                yield break;
            }

            var backend = SelectBackend();
            _worker = WorkerFactory.CreateWorker(backend, _runtimeModel);
            IsModelLoaded = true;
            Debug.Log($"[SkyPlaneAR] Sky model loaded. Backend: {backend}");
        }

        /// <summary>
        /// Runs inference asynchronously. The caller must start this coroutine.
        /// On completion, fires OnInferenceComplete with the output tensor.
        /// Caller is responsible for Tensor.Dispose() on the output.
        /// </summary>
        public IEnumerator RunInferenceAsync(RenderTexture inputFrame)
        {
            if (!IsModelLoaded || _worker == null)
                yield break;

            Tensor inputTensor = null;
            try
            {
                inputTensor = PrepareInputTensor(inputFrame);
                _worker.Execute(inputTensor);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SkyPlaneAR] Inference execute error: {ex.Message}");
                inputTensor?.Dispose();
                yield break;
            }

            // Yield one frame so GPU work can be scheduled without stalling.
            yield return null;

            Tensor outputTensor = null;
            try
            {
                outputTensor = _worker.PeekOutput(OutputLayerName);
                OnInferenceComplete?.Invoke(outputTensor);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SkyPlaneAR] Inference output error: {ex.Message}");
                outputTensor?.Dispose();
            }
            finally
            {
                inputTensor?.Dispose();
            }
        }

        private Tensor PrepareInputTensor(RenderTexture frame)
        {
            int w = _settings.skyModelInputWidth;
            int h = _settings.skyModelInputHeight;

            // Blit to model input size on GPU.
            var resized = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(frame, resized);

            // Convert RenderTexture to Tensor (NHWC: 1 x H x W x 3).
            var tensor = new Tensor(resized, channels: 3);
            RenderTexture.ReleaseTemporary(resized);
            return tensor;
        }

        private WorkerFactory.Type SelectBackend()
        {
            if (SystemInfo.supportsComputeShaders && !Application.isEditor)
                return WorkerFactory.Type.ComputePrecompiled;
            if (SystemInfo.processorCount >= 4)
                return WorkerFactory.Type.CSharpBurst;
            return WorkerFactory.Type.CSharp;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _worker?.Dispose();
        }
    }
#else
    // Stub when Barracuda package is not installed.
    public class BarracudaInferenceRunner : IDisposable
    {
        public bool IsModelLoaded => false;
        public BarracudaInferenceRunner(SkyPlaneARSettings settings) { }
        public System.Collections.IEnumerator LoadModelAsync(string path) { yield break; }
        public System.Collections.IEnumerator RunInferenceAsync(RenderTexture frame) { yield break; }
        public void Dispose() { }
    }
#endif
}
