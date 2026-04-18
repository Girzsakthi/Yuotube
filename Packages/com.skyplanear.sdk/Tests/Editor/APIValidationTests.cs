using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SkyPlaneAR.API;

namespace SkyPlaneAR.Tests.Editor
{
    [Category("SkyPlaneAR")]
    public class APIValidationTests
    {
        private static readonly Type APIType = typeof(SkyPlaneARAPI);

        [Test]
        public void SkyPlaneARAPI_Initialize_MethodExists()
        {
            var method = APIType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.Initialize() must be a public static method.");
        }

        [Test]
        public void SkyPlaneARAPI_OnPlaneDetected_MethodExists()
        {
            var method = APIType.GetMethod("OnPlaneDetected", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.OnPlaneDetected() must be a public static method.");
        }

        [Test]
        public void SkyPlaneARAPI_EnableSkyDetection_MethodExists()
        {
            var method = APIType.GetMethod("EnableSkyDetection", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.EnableSkyDetection() must be a public static method.");
            var param = method.GetParameters();
            Assert.AreEqual(1, param.Length);
            Assert.AreEqual(typeof(bool), param[0].ParameterType);
        }

        [Test]
        public void SkyPlaneARAPI_GetSkyMask_ReturnsResultType()
        {
            var method = APIType.GetMethod("GetSkyMask", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.GetSkyMask() must be a public static method.");
            Assert.AreEqual(typeof(SkyPlaneARResult<Texture2D>), method.ReturnType);
        }

        [Test]
        public void SkyPlaneARAPI_PlaceAnchor_MethodExists()
        {
            var method = APIType.GetMethod("PlaceAnchor", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.PlaceAnchor() must be a public static method.");
        }

        [Test]
        public void SkyPlaneARAPI_Shutdown_MethodExists()
        {
            var method = APIType.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SkyPlaneARAPI.Shutdown() must be a public static method.");
        }

        [Test]
        public void SkyPlaneARAPI_Version_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(SkyPlaneARAPI.Version),
                "SkyPlaneARAPI.Version must be set.");
        }

        [Test]
        public void SkyPlaneAREvents_HasAllRequiredEvents()
        {
            var eventsType = typeof(SkyPlaneAREvents);
            string[] required = {
                "OnSDKInitialized",
                "OnSDKError",
                "OnPlaneDetected",
                "OnPlaneUpdated",
                "OnPlaneRemoved",
                "OnSkyMaskReady",
                "OnCloudDetectionResult"
            };

            foreach (var eventName in required)
            {
                var ev = eventsType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(ev, $"SkyPlaneAREvents.{eventName} event is missing.");
            }
        }

        [Test]
        public void SkyMaskRenderer_ShaderPropertyIDs_MatchKnownValues()
        {
            // Verify shader property names used in C# match what's declared in SkyMask.shader.
            int skyMaskId = UnityEngine.Shader.PropertyToID("_SkyMask");
            int thresholdId = UnityEngine.Shader.PropertyToID("_Threshold");
            int debugColorId = UnityEngine.Shader.PropertyToID("_DebugColor");

            Assert.AreNotEqual(0, skyMaskId);
            Assert.AreNotEqual(0, thresholdId);
            Assert.AreNotEqual(0, debugColorId);

            // IDs should be consistent across calls.
            Assert.AreEqual(skyMaskId, UnityEngine.Shader.PropertyToID("_SkyMask"));
        }
    }
}
