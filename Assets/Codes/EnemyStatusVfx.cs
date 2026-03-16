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
        fireMaterial = CreateMaterial(new Color(1f, 0.48f, 0.08f, 0.95f));
        iceMaterial = CreateMaterial(new Color(0.38f, 0.92f, 1f, 0.95f));
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

        fireCooldown = 0.08f;
        EmitParticles(fireMaterial, "FireSpark");
    }

    public void EmitFrostShards()
    {
        if (iceCooldown > 0f) return;
        iceCooldown = 0.12f;

        Vector3 center = transform.position + Vector3.up * 1.05f;
        if (targetRenderer != null)
            center = targetRenderer.bounds.center + Vector3.up * 0.2f;

        for (int i = 0; i < 6; i++)
        {
            GameObject particleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particleObject.name = "FrostShard";
            particleObject.transform.SetParent(transform, true);
            particleObject.transform.position = center + Random.insideUnitSphere * 0.3f;
            particleObject.transform.localScale = Vector3.one * 0.18f;
            particleObject.transform.rotation = Random.rotation;

            Collider col = particleObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer rend = particleObject.GetComponent<Renderer>();
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.sharedMaterial = iceMaterial;

            Vector3 velocity = new Vector3(
                Random.Range(-0.8f, 0.8f),
                Random.Range(0.8f, 1.8f),
                Random.Range(-0.8f, 0.8f)) * 1.4f;

            activeParticles.Add(new ParticleData
            {
                transform = particleObject.transform,
                velocity = velocity,
                lifetime = 0.55f,
                age = 0f,
                initialScale = particleObject.transform.localScale
            });
        }
    }

    private void EmitParticles(Material material, string particleName)
    {
        Vector3 center = transform.position + Vector3.up * 1.05f;

        if (targetRenderer != null)
        {
            center = targetRenderer.bounds.center + Vector3.up * 0.2f;
        }

        for (int i = 0; i < 7; i++)
        {
            GameObject particleObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particleObject.name = particleName;
            particleObject.transform.SetParent(transform, true);
            particleObject.transform.position = center + Random.insideUnitSphere * 0.38f;
            particleObject.transform.localScale = Vector3.one * 0.28f;

            Collider particleCollider = particleObject.GetComponent<Collider>();
            if (particleCollider != null)
            {
                Destroy(particleCollider);
            }

            Renderer rendererComponent = particleObject.GetComponent<Renderer>();
            rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererComponent.receiveShadows = false;
            rendererComponent.sharedMaterial = material;

            Vector3 velocity = new Vector3(
                Random.Range(-1.1f, 1.1f),
                Random.Range(1.1f, 2.2f),
                Random.Range(-1.1f, 1.1f)) * 1.9f;

            activeParticles.Add(new ParticleData
            {
                transform = particleObject.transform,
                velocity = velocity,
                lifetime = 0.45f,
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
            particle.velocity += Physics.gravity * 0.035f * Time.deltaTime;
            particle.transform.Rotate(180f * Time.deltaTime, 220f * Time.deltaTime, 140f * Time.deltaTime, Space.Self);

            float normalizedLife = 1f - (particle.age / particle.lifetime);
            particle.transform.localScale = particle.initialScale * Mathf.Lerp(0.45f, 1f, normalizedLife);
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

        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * 1.8f);
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

