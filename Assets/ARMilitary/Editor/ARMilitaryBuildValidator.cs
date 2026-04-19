#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.Management;

namespace ARMilitary.Editor
{
    public class ARMilitaryBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) return;

            bool fail = false;

            // Min SDK >= 24
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            {
                Debug.LogError("[ARMilitary] Min SDK must be 24+ for ARCore. Fix: Project Settings > Player > Android > Min SDK.");
                fail = true;
            }

            // IL2CPP scripting backend
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                Debug.LogWarning("[ARMilitary] Recommended: switch Scripting Backend to IL2CPP for ARCore + UDP performance.");
            }

            // ARM64 architecture
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
            {
                Debug.LogError("[ARMilitary] ARM64 must be enabled. Fix: Project Settings > Player > Android > Target Architectures.");
                fail = true;
            }

            // XR Management — ARCore plugin
            var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (xrSettings == null || xrSettings.Manager == null || xrSettings.Manager.activeLoaders.Count == 0)
            {
                Debug.LogError("[ARMilitary] ARCore XR Plugin not enabled. Fix: Project Settings > XR Plugin Management > Android > ARCore.");
                fail = true;
            }

            // URP asset assigned
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
            {
                Debug.LogError("[ARMilitary] No Render Pipeline Asset assigned. Fix: Project Settings > Graphics > Scriptable Render Pipeline Settings.");
                fail = true;
            }

            if (!fail)
                Debug.Log("[ARMilitary] Build validation passed.");
        }
    }
}
#endif
