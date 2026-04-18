using UnityEngine;
using SkyPlaneAR.Core;

namespace SkyPlaneAR.Rendering
{
    [DisallowMultipleComponent]
    public class SkyMaskRenderer : MonoBehaviour
    {
        private Material _skyMaskMaterial;
        private MaterialPropertyBlock _propBlock;

        private static readonly int SkyMaskTexID = Shader.PropertyToID("_SkyMask");
        private static readonly int ThresholdID   = Shader.PropertyToID("_Threshold");
        private static readonly int DebugColorID  = Shader.PropertyToID("_DebugColor");

        private bool _visualizationEnabled;
        private SkyPlaneARSettings _settings;

        public void Initialize(SkyPlaneARSettings settings)
        {
            _settings = settings;
            _propBlock = new MaterialPropertyBlock();

            var shader = Shader.Find("SkyPlaneAR/SkyMask");
            if (shader != null)
                _skyMaskMaterial = new Material(shader);
            else
                Debug.LogWarning("[SkyPlaneAR] SkyMask shader not found. Visualization disabled.");

            SetVisualizationEnabled(settings.enableSkyMaskVisualization);
        }

        public void UpdateSkyMask(Texture2D mask)
        {
            if (mask == null || _skyMaskMaterial == null) return;

            _skyMaskMaterial.SetTexture(SkyMaskTexID, mask);
            _skyMaskMaterial.SetFloat(ThresholdID, _settings.skyMaskThreshold);
            _skyMaskMaterial.SetColor(DebugColorID, _settings.skyMaskDebugColor);
        }

        public void SetVisualizationEnabled(bool enabled)
        {
            _visualizationEnabled = enabled;
            if (_skyMaskMaterial != null)
                _skyMaskMaterial.SetFloat("_EnableDebug", enabled ? 1f : 0f);
        }

        public Material GetSkyMaskMaterial() => _skyMaskMaterial;

        private void OnDestroy()
        {
            if (_skyMaskMaterial != null)
                Destroy(_skyMaskMaterial);
        }
    }
}
