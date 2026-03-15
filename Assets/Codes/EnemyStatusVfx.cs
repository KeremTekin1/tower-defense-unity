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
        fireMaterial = CreateMaterial(new Color(1f, 0.42f, 0.05f, 0.85f));
        iceMaterial = CreateMaterial(new Color(0.45f, 0.88f, 1f, 0.8f));
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        iceCooldown -= Time.deltaTime;
        UpdateParticles();
    }

    public void EmitFireSparks()
    {
        if (fireCooldown > 0f)
        {
            return;
        }

        fireCooldown = 0.12f;
        EmitParticles(PrimitiveType.Sphere, fireMaterial, 4, 0.32f, 0.16f, 1.1f);
    }

    public void EmitFrostShards()
    {
        if (iceCooldown > 0f)
        {
            return;
        }

        iceCooldown = 0.18f;
        EmitParticles(PrimitiveType.Cube, iceMaterial, 5, 0.38f, 0.12f, 0.95f);
    }

    private void EmitParticles(PrimitiveType primitiveType, Material material, int count, float lifetime, float scale, float heightBias)
    {
        Vector3 center = transform.position + Vector3.up * heightBias;

        if (targetRenderer != null)
        {
            center = targetRenderer.bounds.center + Vector3.up * 0.15f;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject particleObject = GameObject.CreatePrimitive(primitiveType);
            particleObject.name = primitiveType == PrimitiveType.Sphere ? "FireSpark" : "FrostShard";
            particleObject.transform.SetParent(transform, true);
            particleObject.transform.position = center + Random.insideUnitSphere * 0.22f;
            particleObject.transform.localScale = Vector3.one * scale;

            Collider particleCollider = particleObject.GetComponent<Collider>();
            if (particleCollider != null)
            {
                Destroy(particleCollider);
            }

            Renderer rendererComponent = particleObject.GetComponent<Renderer>();
            rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererComponent.receiveShadows = false;
            rendererComponent.sharedMaterial = material;

            Vector3 velocity = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(0.9f, 1.6f), Random.Range(-0.7f, 0.7f));
            if (primitiveType == PrimitiveType.Cube)
            {
                particleObject.transform.rotation = Random.rotation;
                velocity += new Vector3(0f, 0.2f, 0f);
            }

            activeParticles.Add(new ParticleData
            {
                transform = particleObject.transform,
                velocity = velocity,
                lifetime = lifetime,
                age = 0f,
                initialScale = particleObject.transform.localScale
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
            particle.velocity += Physics.gravity * 0.08f * Time.deltaTime;
            particle.transform.Rotate(90f * Time.deltaTime, 120f * Time.deltaTime, 60f * Time.deltaTime, Space.Self);

            float normalizedLife = 1f - (particle.age / particle.lifetime);
            particle.transform.localScale = particle.initialScale * Mathf.Lerp(0.35f, 1f, normalizedLife);
        }
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.renderQueue = 3000;
        return material;
    }
}
