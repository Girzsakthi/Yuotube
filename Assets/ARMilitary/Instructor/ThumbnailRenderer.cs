using ARMilitary.Data;
using ARMilitary.Primitives;
using UnityEngine;

namespace ARMilitary.Instructor
{
    public static class ThumbnailRenderer
    {
        public static Texture2D Render(ObjectType type, int width, int height)
        {
            // Off-screen camera setup
            var camGO = new GameObject("ThumbnailCam_" + type);
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.07f, 0.12f, 0.07f, 1f);
            cam.orthographic     = false;
            cam.fieldOfView      = 45f;
            cam.nearClipPlane    = 0.01f;
            cam.farClipPlane     = 50f;
            cam.cullingMask      = 1 << 31; // exclusive layer (avoid polluting main scene)

            var rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            // Build preview object on layer 31
            var previewGO = PrimitiveFactory.Build(type, null);
            SetLayer(previewGO, 31);

            // Frame the object
            var bounds = GetBounds(previewGO);
            float dist = bounds.size.magnitude * 1.5f;
            camGO.transform.position = bounds.center + new Vector3(0.5f, 0.6f, -1f).normalized * dist;
            cam.transform.LookAt(bounds.center);

            // Lighting
            var lightGO = new GameObject("ThumbLight");
            lightGO.layer = 31;
            var light = lightGO.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(45, -45, 0);

            // Render
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            // Cleanup
            Object.DestroyImmediate(previewGO);
            Object.DestroyImmediate(lightGO);
            Object.DestroyImmediate(camGO);
            Object.DestroyImmediate(rt);

            return tex;
        }

        private static Bounds GetBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        private static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayer(child.gameObject, layer);
        }
    }
}
