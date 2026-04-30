using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusVfx : MonoBehaviour
{
    private class ParticleData
    {
        public Transform transform;
        public Vector3 velocity;
        public float lifetime;
        public float age;
        public Vector3 initialScale;
    }

    private readonly List<ParticleData> activeParticles = new List<ParticleData>();

    private Material fireMaterial;
    private Material iceMaterial;
    private Renderer targetRenderer;
    private float fireCooldown;
    private float iceCooldown;

    private void Awake()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
        fireMaterial = TowerVisualHelper.CreateEmissiveMaterial(new Color(1f, 0.48f, 0.08f, 0.95f));
        iceMaterial = TowerVisualHelper.CreateEmissiveMaterial(new Color(0.38f, 0.92f, 1f, 0.95f));
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        iceCooldown -= Time.deltaTime;
        UpdateParticles();
    }

    public void EmitFireSparks()
    {
        if (fireCooldown > 0f) return;

        fireCooldown = 0.08f;
        EmitParticles(fireMaterial, PrimitiveType.Sphere, 7, 0.28f, 0.45f, 1.9f);
    }

    public void EmitFrostShards()
    {
        if (iceCooldown > 0f) return;

        iceCooldown = 0.12f;
        EmitParticles(iceMaterial, PrimitiveType.Cube, 6, 0.18f, 0.55f, 1.4f, randomRotation: true);
    }

    private void EmitParticles(Material material, PrimitiveType shape, int count, float scale, float lifetime, float speedMultiplier, bool randomRotation = false)
    {
        Vector3 center = targetRenderer != null
            ? targetRenderer.bounds.center + Vector3.up * 0.2f
            : transform.position + Vector3.up * 1.05f;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(shape);
            obj.transform.SetParent(transform, true);
            obj.transform.position = center + Random.insideUnitSphere * 0.38f;
            obj.transform.localScale = Vector3.one * scale;

            if (randomRotation)
                obj.transform.rotation = Random.rotation;

            Collider col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer rend = obj.GetComponent<Renderer>();
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.sharedMaterial = material;

            Vector3 velocity = new Vector3(
                Random.Range(-1.1f, 1.1f),
                Random.Range(1.1f, 2.2f),
                Random.Range(-1.1f, 1.1f)) * speedMultiplier;

            activeParticles.Add(new ParticleData
            {
                transform = obj.transform,
                velocity = velocity,
                lifetime = lifetime,
                age = 0f,
                initialScale = obj.transform.localScale
            });
        }
    }

    private void UpdateParticles()
    {
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            ParticleData particle = activeParticles[i];
            if (particle.transform == null)
            {
                activeParticles.RemoveAt(i);
                continue;
            }

            particle.age += Time.deltaTime;
            if (particle.age >= particle.lifetime)
            {
                Destroy(particle.transform.gameObject);
                activeParticles.RemoveAt(i);
                continue;
            }

            particle.transform.position += particle.velocity * Time.deltaTime;
            particle.velocity += Physics.gravity * 0.035f * Time.deltaTime;
            particle.transform.Rotate(180f * Time.deltaTime, 220f * Time.deltaTime, 140f * Time.deltaTime, Space.Self);

            float normalizedLife = 1f - (particle.age / particle.lifetime);
            particle.transform.localScale = particle.initialScale * Mathf.Lerp(0.45f, 1f, normalizedLife);
        }
    }
}
