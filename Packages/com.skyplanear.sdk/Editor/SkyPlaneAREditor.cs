using System;
using UnityEditor;
using UnityEngine;

namespace SkyPlaneAR.Editor
{
    public class SkyPlaneAREditor : EditorWindow
    {
        private const string PackageVersion = "1.0.0";

        [MenuItem("Window/SkyPlaneAR")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkyPlaneAREditor>("SkyPlaneAR SDK");
            window.minSize = new Vector2(380, 300);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"SkyPlaneAR SDK  v{PackageVersion}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Production AR SDK for Unity", EditorStyles.miniLabel);
            EditorGUILayout.Space(4);

            DrawSeparator();
            DrawDependencyStatus();
            DrawSeparator();
            DrawSampleButtons();
            DrawSeparator();
            DrawExportButton();
            DrawSeparator();
            DrawLinks();
        }

        private void DrawDependencyStatus()
        {
            EditorGUILayout.LabelField("Package Dependencies", EditorStyles.boldLabel);
            DrawDepRow("AR Foundation 5.x",  IsDefined("SKYPLANEAR_ARFOUNDATION"));
            DrawDepRow("Barracuda 3.x",      IsDefined("SKYPLANEAR_BARRACUDA"));
            DrawDepRow("URP 14.x",           IsPackageInstalled("com.unity.render-pipelines.universal"));
        }

        private void DrawDepRow(string label, bool ok)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(200));
            var prevColor = GUI.color;
            GUI.color = ok ? Color.green : Color.red;
            EditorGUILayout.LabelField(ok ? "✓ Installed" : "✗ Missing");
            GUI.color = prevColor;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSampleButtons()
        {
            EditorGUILayout.LabelField("Sample Scenes", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Basic AR Sample (Plane Spawn)"))
                OpenSample("Assets/SkyPlaneAR/Samples/BasicARSample");
            if (GUILayout.Button("Open Sky Overlay Sample"))
                OpenSample("Assets/SkyPlaneAR/Samples/SkyOverlaySample");
        }

        private void DrawExportButton()
        {
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            if (GUILayout.Button("Export SkyPlaneAR SDK as .unitypackage"))
                ExportUnityPackage();
        }

        private void DrawLinks()
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
            if (GUILayout.Button("Documentation")) Application.OpenURL("https://skyplanear.io/docs");
            if (GUILayout.Button("Report Issue"))  Application.OpenURL("https://github.com/skyplanear/sdk/issues");
        }

        private void OpenSample(string path)
        {
            var scene = AssetDatabase.FindAssets("t:Scene", new[] { path });
            if (scene.Length == 0)
            {
                EditorUtility.DisplayDialog("SkyPlaneAR", $"Sample scene not found at {path}", "OK");
                return;
            }
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                AssetDatabase.GUIDToAssetPath(scene[0]));
        }

        public static void ExportUnityPackage()
        {
            string outputPath = EditorUtility.SaveFilePanel(
                "Export SkyPlaneAR SDK",
                "",
                $"SkyPlaneAR_SDK_v{PackageVersion}.unitypackage",
                "unitypackage");

            if (string.IsNullOrEmpty(outputPath)) return;

            AssetDatabase.ExportPackage(
                new[] { "Assets/SkyPlaneAR", "Packages/com.skyplanear.sdk" },
                outputPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            EditorUtility.DisplayDialog("SkyPlaneAR", $"Exported to:\n{outputPath}", "OK");
        }

        /// <summary>CLI entry point: Unity -executeMethod SkyPlaneAR.Editor.SkyPlaneAREditor.ExportUnityPackageCLI</summary>
        public static void ExportUnityPackageCLI()
        {
            var args = System.Environment.GetCommandLineArgs();
            string outputPath = $"SkyPlaneAR_SDK_v{PackageVersion}.unitypackage";

            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-outputPath") outputPath = args[i + 1];

            AssetDatabase.ExportPackage(
                new[] { "Assets/SkyPlaneAR", "Packages/com.skyplanear.sdk" },
                outputPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            Debug.Log($"[SkyPlaneAR] Package exported to {outputPath}");
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space(4);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1f));
            EditorGUILayout.Space(4);
        }

        private static bool IsDefined(string symbol)
        {
#if SKYPLANEAR_ARFOUNDATION
            if (symbol == "SKYPLANEAR_ARFOUNDATION") return true;
#endif
#if SKYPLANEAR_BARRACUDA
            if (symbol == "SKYPLANEAR_BARRACUDA") return true;
#endif
            return false;
        }

        private static bool IsPackageInstalled(string packageName)
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true);
            while (!listRequest.IsCompleted) { }
            if (listRequest.Status != UnityEditor.PackageManager.StatusCode.Success) return false;
            foreach (var pkg in listRequest.Result)
                if (pkg.name == packageName) return true;
            return false;
        }
    }
}
