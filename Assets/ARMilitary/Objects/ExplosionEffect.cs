using System.Collections;
using UnityEngine;

namespace ARMilitary.Objects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float destroyDelay = 3f;

        private void Start()
        {
            var ps = GetComponent<ParticleSystem>();
            ConfigureParticles(ps);
            ps.Play();
            StartCoroutine(DestroyAfterDelay());
        }

        private static void ConfigureParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 4f;
            main.startSize = 0.3f;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.5f, 0.1f), new Color(1f, 0.2f, 0f));
            main.maxParticles = 80;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.red, 0.5f), new GradientColorKey(Color.gray, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(gradient);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            var sizeCurve = new AnimationCurve(new Keyframe(0, 0.3f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0.1f));
            size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        public static GameObject Spawn(Vector3 worldPosition)
        {
            var go = new GameObject("Explosion");
            go.transform.position = worldPosition;
            go.AddComponent<ParticleSystem>();
            go.AddComponent<ExplosionEffect>();
            return go;
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            Destroy(gameObject);
        }
    }
}
