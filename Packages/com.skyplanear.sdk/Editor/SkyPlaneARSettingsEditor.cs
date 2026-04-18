using UnityEditor;
using UnityEngine;
using SkyPlaneAR.Core;

namespace SkyPlaneAR.Editor
{
    [CustomEditor(typeof(SkyPlaneARSettings))]
    public class SkyPlaneARSettingsEditor : UnityEditor.Editor
    {
        private bool _performanceFoldout = true;
        private bool _skyFoldout = true;
        private bool _planeFoldout = true;
        private bool _cloudFoldout = false;
        private bool _renderingFoldout = false;
        private bool _trackingFoldout = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("SkyPlaneAR SDK Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawPerformanceSection();
            DrawSkyDetectionSection();
            DrawPlaneDetectionSection();
            DrawCloudDetectionSection();
            DrawRenderingSection();
            DrawTrackingSection();

            EditorGUILayout.Space(8);
            DrawValidateButton();
            DrawPerformanceEstimate();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPerformanceSection()
        {
            _performanceFoldout = EditorGUILayout.Foldout(_performanceFoldout, "Performance", true, EditorStyles.foldoutHeader);
            if (!_performanceFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetFrameRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inferenceThrottleFrames"),
                new GUIContent("Inference Throttle (frames)", "Run ML every N frames. Higher = better perf."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("texturePoolSize"));
            EditorGUI.indentLevel--;
        }

        private void DrawSkyDetectionSection()
        {
            _skyFoldout = EditorGUILayout.Foldout(_skyFoldout, "Sky Detection", true, EditorStyles.foldoutHeader);
            if (!_skyFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyModelInputWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyModelInputHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyMaskThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableSkyDetectionOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyModelRelativePath"));
            EditorGUI.indentLevel--;
        }

        private void DrawPlaneDetectionSection()
        {
            _planeFoldout = EditorGUILayout.Foldout(_planeFoldout, "Plane Detection", true, EditorStyles.foldoutHeader);
            if (!_planeFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectHorizontalPlanes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectVerticalPlanes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumPlaneArea"),
                new GUIContent("Min Plane Area (m²)"));
            EditorGUI.indentLevel--;
        }

        private void DrawCloudDetectionSection()
        {
            _cloudFoldout = EditorGUILayout.Foldout(_cloudFoldout, "Cloud Detection", true, EditorStyles.foldoutHeader);
            if (!_cloudFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableCloudDetection"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudConfidenceThreshold"));
            EditorGUI.indentLevel--;
        }

        private void DrawRenderingSection()
        {
            _renderingFoldout = EditorGUILayout.Foldout(_renderingFoldout, "Rendering", true, EditorStyles.foldoutHeader);
            if (!_renderingFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableSkyMaskVisualization"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyMaskDebugColor"));
            EditorGUI.indentLevel--;
        }

        private void DrawTrackingSection()
        {
            _trackingFoldout = EditorGUILayout.Foldout(_trackingFoldout, "Tracking", true, EditorStyles.foldoutHeader);
            if (!_trackingFoldout) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorPositionSmoothing"),
                new GUIContent("Position Smoothing", "EMA alpha for position. Lower = smoother."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorRotationSmoothing"),
                new GUIContent("Rotation Smoothing", "EMA alpha for rotation."));
            EditorGUI.indentLevel--;
        }

        private void DrawValidateButton()
        {
            if (GUILayout.Button("Validate SDK Dependencies"))
                ValidateDependencies();
        }

        private void DrawPerformanceEstimate()
        {
            var s = (SkyPlaneARSettings)target;
            if (s.targetFrameRate <= 0) return;

            float inferenceHz = (float)s.targetFrameRate / s.inferenceThrottleFrames;
            EditorGUILayout.HelpBox(
                $"Estimated inference rate: {inferenceHz:F1} Hz " +
                $"({s.inferenceThrottleFrames} frame throttle at {s.targetFrameRate} FPS)",
                MessageType.Info);
        }

        private void ValidateDependencies()
        {
            bool arFoundationOk = IsPackageInstalled("com.unity.xr.arfoundation");
            bool barracudaOk    = IsPackageInstalled("com.unity.barracuda");
            bool urpOk          = IsPackageInstalled("com.unity.render-pipelines.universal");

            string report = "SkyPlaneAR Dependency Check:\n" +
                $"  AR Foundation : {(arFoundationOk ? "OK" : "MISSING")}\n" +
                $"  Barracuda     : {(barracudaOk    ? "OK" : "MISSING")}\n" +
                $"  URP           : {(urpOk          ? "OK" : "MISSING")}";

            if (arFoundationOk && barracudaOk && urpOk)
                EditorUtility.DisplayDialog("SkyPlaneAR", report + "\n\nAll dependencies satisfied.", "OK");
            else
                EditorUtility.DisplayDialog("SkyPlaneAR", report + "\n\nInstall missing packages via Package Manager.", "OK");

            Debug.Log("[SkyPlaneAR] " + report);
        }

        private bool IsPackageInstalled(string packageName)
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true);
            // Synchronous polling for editor validation (not runtime).
            while (!listRequest.IsCompleted) { }
            if (listRequest.Status != UnityEditor.PackageManager.StatusCode.Success) return false;
            foreach (var pkg in listRequest.Result)
                if (pkg.name == packageName) return true;
            return false;
        }
    }
}
