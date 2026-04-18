using UnityEditor;
using UnityEngine;

namespace SkyPlaneAR.Editor
{
    /// <summary>
    /// Automatically configures Barracuda NNModel settings when an .onnx file
    /// is imported into the SkyDetection/Models/ directory.
    /// </summary>
    public class OnnxModelImportPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".onnx", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!path.Contains("SkyDetection/Models"))
                    continue;

                Debug.Log($"[SkyPlaneAR] ONNX model imported: {path}");
                ValidateOnnxModel(path);
            }
        }

        private static void ValidateOnnxModel(string assetPath)
        {
#if SKYPLANEAR_BARRACUDA
            var nnModel = AssetDatabase.LoadAssetAtPath<Unity.Barracuda.NNModel>(assetPath);
            if (nnModel == null)
            {
                Debug.LogWarning($"[SkyPlaneAR] Could not load NNModel at {assetPath}. " +
                                  "Ensure Barracuda package is installed.");
                return;
            }

            Debug.Log($"[SkyPlaneAR] Model '{nnModel.name}' validated successfully. " +
                      "Assign it to SkyPlaneARSettings.skyModelRelativePath.");
#else
            Debug.LogWarning("[SkyPlaneAR] Barracuda package not installed. Cannot validate ONNX model.");
#endif
        }
    }
}
