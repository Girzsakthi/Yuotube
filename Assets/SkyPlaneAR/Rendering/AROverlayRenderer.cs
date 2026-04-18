using System.Collections.Generic;
using UnityEngine;
using SkyPlaneAR.Core;

namespace SkyPlaneAR.Rendering
{
    [DisallowMultipleComponent]
    public class AROverlayRenderer : MonoBehaviour
    {
        private readonly List<(GameObject go, Material mat)> _overlayObjects =
            new List<(GameObject, Material)>();

        private static readonly int SkyMaskTexID = Shader.PropertyToID("_SkyMask");
        private static readonly int ThresholdID   = Shader.PropertyToID("_Threshold");

        private SkyPlaneARSettings _settings;

        public void Initialize(SkyPlaneARSettings settings, SkyMaskRenderer skyRenderer)
        {
            _settings = settings;
        }

        /// <summary>
        /// Registers a GameObject whose material should respect the sky mask.
        /// Pass the material that uses the SkyPlaneAR/AROverlay shader.
        /// </summary>
        public void RegisterOverlayObject(GameObject go, Material overlayMaterial)
        {
            if (go == null || overlayMaterial == null) return;
            _overlayObjects.Add((go, overlayMaterial));
        }

        public void UnregisterOverlayObject(GameObject go)
        {
            _overlayObjects.RemoveAll(pair => pair.go == go);
        }

        /// <summary>Called by SkyMaskRenderer when a new sky mask is ready.</summary>
        public void UpdateMaskOnMaterials(Texture2D skyMask)
        {
            if (skyMask == null) return;

            foreach (var (go, mat) in _overlayObjects)
            {
                if (go == null || mat == null) continue;
                mat.SetTexture(SkyMaskTexID, skyMask);
                mat.SetFloat(ThresholdID, _settings.skyMaskThreshold);
            }
        }

        private void OnDestroy()
        {
            _overlayObjects.Clear();
        }
    }
}
