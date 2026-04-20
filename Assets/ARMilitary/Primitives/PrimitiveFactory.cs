using ARMilitary.Data;
using ARMilitary.Objects;
using UnityEngine;

namespace ARMilitary.Primitives
{
    public static class PrimitiveFactory
    {
        public static GameObject Build(ObjectType type, Transform parent)
        {
            var root = new GameObject(type.ToString());
            root.transform.SetParent(parent, false);

            switch (type)
            {
                case ObjectType.Drone:  BuildDrone(root.transform);  break;
                case ObjectType.Jet:    BuildJet(root.transform);    break;
                case ObjectType.Tanker: BuildTanker(root.transform); break;
                case ObjectType.Bunker: BuildBunker(root.transform); break;
            }

            // Attach behaviour component
            var obj = root.AddComponent<ARMilitaryObject>();
            obj.ObjectType = type;
            return root;
        }

        // ─── Drone: central sphere body + 4 rotor arms + 4 rotor discs ─────────
        private static void BuildDrone(Transform parent)
        {
            var mat = MakeMat(new Color(0.2f, 0.8f, 0.2f));

            var body = Prim(PrimitiveType.Sphere, parent, Vector3.zero, new Vector3(0.3f, 0.15f, 0.3f), mat);
            body.name = "Body";

            float armLen = 0.35f;
            Vector3[] armDirs = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            foreach (var dir in armDirs)
            {
                var arm = Prim(PrimitiveType.Cylinder, parent, dir * armLen * 0.5f,
                               new Vector3(0.05f, armLen * 0.5f, 0.05f), mat);
                arm.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);

                var rotor = Prim(PrimitiveType.Cylinder, parent, dir * armLen + Vector3.up * 0.05f,
                                 new Vector3(0.22f, 0.02f, 0.22f), MakeMat(new Color(0.1f, 0.1f, 0.1f)));
                rotor.name = "Rotor";
                rotor.AddComponent<RotorSpin>();
            }

            parent.gameObject.AddComponent<DroneAnimator>();
        }

        // ─── Jet: elongated fuselage + swept wings + tail fins ──────────────────
        private static void BuildJet(Transform parent)
        {
            var mat = MakeMat(new Color(0.3f, 0.5f, 0.9f));
            var darkMat = MakeMat(new Color(0.15f, 0.25f, 0.5f));

            Prim(PrimitiveType.Capsule, parent, Vector3.zero,  new Vector3(0.2f, 0.8f, 0.2f), mat).name = "Fuselage";

            // Wings (flat cubes, swept back)
            var leftWing = Prim(PrimitiveType.Cube, parent, new Vector3(-0.5f, 0, -0.1f),
                                new Vector3(0.6f, 0.04f, 0.25f), mat);
            leftWing.transform.localRotation = Quaternion.Euler(0, -15, 0);

            var rightWing = Prim(PrimitiveType.Cube, parent, new Vector3(0.5f, 0, -0.1f),
                                 new Vector3(0.6f, 0.04f, 0.25f), mat);
            rightWing.transform.localRotation = Quaternion.Euler(0, 15, 0);

            // Tail fins
            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.15f, -0.7f), new Vector3(0.04f, 0.2f, 0.15f), darkMat).name = "TailFin";
            Prim(PrimitiveType.Cube, parent, new Vector3(-0.18f, 0, -0.7f), new Vector3(0.2f, 0.04f, 0.12f), darkMat);
            Prim(PrimitiveType.Cube, parent, new Vector3(0.18f, 0, -0.7f),  new Vector3(0.2f, 0.04f, 0.12f), darkMat);

            // Engine exhaust glow
            var exhaust = Prim(PrimitiveType.Cylinder, parent, new Vector3(0, 0, -0.85f),
                               new Vector3(0.08f, 0.08f, 0.08f), MakeMat(new Color(1f, 0.5f, 0.1f)));
            exhaust.name = "Exhaust";

            parent.transform.localRotation = Quaternion.Euler(90, 0, 0);
            parent.gameObject.AddComponent<JetAnimator>();
        }

        // ─── Tanker: box hull + cylinder tank + cab + wheels ────────────────────
        private static void BuildTanker(Transform parent)
        {
            var bodyMat  = MakeMat(new Color(0.4f, 0.35f, 0.2f));
            var tankMat  = MakeMat(new Color(0.55f, 0.5f, 0.35f));
            var wheelMat = MakeMat(new Color(0.1f, 0.1f, 0.1f));

            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.35f, 0), new Vector3(0.8f, 0.45f, 1.8f), bodyMat).name = "Hull";
            Prim(PrimitiveType.Cylinder, parent, new Vector3(0, 0.75f, 0), new Vector3(0.4f, 0.7f, 0.4f), tankMat).name = "Tank";
            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.55f, 1.1f), new Vector3(0.75f, 0.55f, 0.5f), tankMat).name = "Cab";

            Vector3[] wheelPos = {
                new Vector3(-0.45f, 0.12f,  0.7f), new Vector3(0.45f, 0.12f,  0.7f),
                new Vector3(-0.45f, 0.12f,  0f),   new Vector3(0.45f, 0.12f,  0f),
                new Vector3(-0.45f, 0.12f, -0.7f), new Vector3(0.45f, 0.12f, -0.7f)
            };
            foreach (var wp in wheelPos)
            {
                var w = Prim(PrimitiveType.Cylinder, parent, wp, new Vector3(0.22f, 0.1f, 0.22f), wheelMat);
                w.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }

        // ─── Bunker: squat concrete box + sandbag berm + gun slit ───────────────
        private static void BuildBunker(Transform parent)
        {
            var concreteMat = MakeMat(new Color(0.5f, 0.5f, 0.5f));
            var sandMat     = MakeMat(new Color(0.7f, 0.65f, 0.45f));
            var darkMat     = MakeMat(new Color(0.1f, 0.1f, 0.1f));

            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.25f, 0), new Vector3(1.2f, 0.5f, 1.0f), concreteMat).name = "Wall";
            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.52f, 0), new Vector3(1.25f, 0.06f, 1.05f), concreteMat).name = "Roof";

            // Sandbag berm (front)
            for (int i = -2; i <= 2; i++)
                Prim(PrimitiveType.Sphere, parent, new Vector3(i * 0.22f, 0.12f, 0.55f),
                     new Vector3(0.22f, 0.18f, 0.22f), sandMat);

            // Gun slit
            Prim(PrimitiveType.Cube, parent, new Vector3(0, 0.3f, 0.51f), new Vector3(0.3f, 0.08f, 0.02f), darkMat).name = "Slit";
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        private static GameObject Prim(PrimitiveType type, Transform parent,
                                       Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        private static Material MakeMat(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard")
                      ?? Shader.Find("Hidden/InternalErrorShader");
            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }
    }
}
