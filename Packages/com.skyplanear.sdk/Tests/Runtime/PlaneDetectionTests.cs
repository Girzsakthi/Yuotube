using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SkyPlaneAR.PlaneDetection;

namespace SkyPlaneAR.Tests
{
    [Category("SkyPlaneAR")]
    public class PlaneDetectionTests
    {
        [Test]
        public void FallbackPlaneDetector_IsAlwaysSupported()
        {
            var detector = new FallbackPlaneDetector();
            Assert.IsTrue(detector.IsSupported);
        }

        [Test]
        public void FallbackPlaneDetector_EmitsPlaneAddedOnFirstTick()
        {
            var detector = new FallbackPlaneDetector();
            var config = new PlaneDetectionConfig(true, true, 0.01f);
            detector.StartDetection(config);

            int addedCount = 0;
            detector.OnPlaneAdded += _ => addedCount++;
            detector.Tick();

            Assert.GreaterOrEqual(addedCount, 1, "FallbackPlaneDetector should emit at least one plane on first Tick.");
        }

        [Test]
        public void FallbackPlaneDetector_DoesNotEmitTwice()
        {
            var detector = new FallbackPlaneDetector();
            var config = new PlaneDetectionConfig(true, true, 0.01f);
            detector.StartDetection(config);

            int addedCount = 0;
            detector.OnPlaneAdded += _ => addedCount++;

            detector.Tick();
            int afterFirstTick = addedCount;
            detector.Tick();

            Assert.AreEqual(afterFirstTick, addedCount, "FallbackPlaneDetector should not emit planes on subsequent ticks.");
        }

        [Test]
        public void FallbackPlaneDetector_HorizontalPlane_HasCorrectAlignment()
        {
            var detector = new FallbackPlaneDetector();
            var config = new PlaneDetectionConfig(true, false, 0.01f);
            detector.StartDetection(config);

            PlaneData? receivedPlane = null;
            detector.OnPlaneAdded += p => receivedPlane = p;
            detector.Tick();

            Assert.IsTrue(receivedPlane.HasValue, "No plane was emitted.");
            Assert.AreEqual(PlaneAlignment.HorizontalUp, receivedPlane.Value.Alignment);
            Assert.Greater(receivedPlane.Value.Area, 0f);
            Assert.IsNotNull(receivedPlane.Value.BoundaryPoints);
            Assert.Greater(receivedPlane.Value.BoundaryPoints.Length, 0);
        }

        [Test]
        public void PlaneData_IsImmutableStruct()
        {
            var plane = new PlaneData("test_id", Vector3.zero, Vector3.up,
                                      new Vector2(1f, 1f), PlaneAlignment.HorizontalUp,
                                      Pose.identity, new Vector2[0]);

            // Copy struct and verify independence.
            PlaneData copy = plane;
            Assert.AreEqual(plane.Id, copy.Id);
            Assert.AreEqual(plane.Center, copy.Center);
            Assert.AreEqual(plane.Alignment, copy.Alignment);
        }

        [Test]
        public void PlaneDetectionManager_StoresDetectedPlanes()
        {
            var settings = ScriptableObject.CreateInstance<Core.SkyPlaneARSettings>();
            settings.detectHorizontalPlanes = true;
            settings.detectVerticalPlanes = true;
            settings.minimumPlaneArea = 0.01f;

            var manager = new PlaneDetectionManager();
            manager.Initialize(settings);
            manager.StartDetection();
            manager.Tick(0.016f);

            Assert.GreaterOrEqual(manager.DetectedPlanes.Count, 1,
                "PlaneDetectionManager should store planes emitted by fallback detector.");

            manager.Shutdown();
            Object.DestroyImmediate(settings);
        }
    }
}
