using System.Collections.Generic;
using UnityEngine;

public class SlowTower : MonoBehaviour, ITower
{
    public float range = 8f;
    public float coneAngle = 90f;
    public float fireRate = 1.2f;
    [HideInInspector]
    public float damage = 18f;
    public float slowMultiplier = 0.45f;
    public float slowDuration = 1.8f;
    public int visualSegments = 24;

    private float fireCooldown;
    private MeshFilter areaMeshFilter;
    private MeshRenderer areaMeshRenderer;
    private Mesh areaMesh;
    private bool statsCached;
    private float baseRange;
    private float baseFireRate;
    private float baseSlowMultiplier;
    private float baseSlowDuration;
    private int level = 1;

    public int Level => level;

    private void Awake()
    {
        CacheBaseStats();
        CreateAreaVisual();
        ApplyLevelStats();
        UpdateAreaVisual();
    }

    private void OnValidate()
    {
        coneAngle = Mathf.Clamp(coneAngle, 1f, 180f);
        range = Mathf.Max(0.1f, range);
        fireRate = Mathf.Max(0.1f, fireRate);
        visualSegments = Mathf.Clamp(visualSegments, 3, 64);

        if (Application.isPlaying)
        {
            UpdateAreaVisual();
            return;
        }

        CreateAreaVisual();
        UpdateAreaVisual();
    }

    private void Update()
    {
        TickCooldown();
        TryShoot();
    }

    public bool CanUpgrade()
    {
        return level < 3;
    }

    public int GetUpgradeCost()
    {
        if (level == 1)
        {
            return 50;
        }

        if (level == 2)
        {
            return 90;
        }

        return 0;
    }

    public bool TryUpgrade()
    {
        CacheBaseStats();

        if (!CanUpgrade())
        {
            return false;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.SpendMoney(GetUpgradeCost()))
        {
            return false;
        }

        level++;
        ApplyLevelStats();
        UpdateAreaVisual();
        return true;
    }

    private void CacheBaseStats()
    {
        if (statsCached)
        {
            return;
        }

        baseRange = range;
        baseFireRate = fireRate;
        baseSlowMultiplier = slowMultiplier;
        baseSlowDuration = slowDuration;
        statsCached = true;
    }

    private void ApplyLevelStats()
    {
        int index = Mathf.Clamp(level - 1, 0, 2);
        float[] rangeMultipliers = { 1f, 1.14f, 1.3f };
        float[] fireRateMultipliers = { 1f, 1.2f, 1.4f };
        float[] slowDurationMultipliers = { 1f, 1.2f, 1.38f };
        float[] slowStrengthMultipliers = { 1f, 0.82f, 0.64f };

        range = baseRange * rangeMultipliers[index];
        fireRate = baseFireRate * fireRateMultipliers[index];
        slowDuration = baseSlowDuration * slowDurationMultipliers[index];
        slowMultiplier = Mathf.Clamp(baseSlowMultiplier * slowStrengthMultipliers[index], 0.15f, 1f);
    }

    private void TickCooldown()
    {
        fireCooldown -= Time.deltaTime;
    }

    private void TryShoot()
    {
        if (fireCooldown > 0f)
        {
            return;
        }

        if (!HasEnemyInCone())
        {
            return;
        }

        ApplyConeEffect();
        fireCooldown = 1f / fireRate;
    }

    private bool HasEnemyInCone()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth enemyHealth = enemies[i];
            if (enemyHealth != null && !enemyHealth.IsDead && IsInsideCone(enemyHealth.transform.position))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyConeEffect()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth enemyHealth = enemies[i];
            if (enemyHealth == null || enemyHealth.IsDead || !IsInsideCone(enemyHealth.transform.position))
            {
                continue;
            }

            EnemyMovement movement = enemyHealth.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.ApplySlow(slowMultiplier, slowDuration);
                enemyHealth.PlaySlowFeedback();
            }
        }
    }

    private bool IsInsideCone(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > range * range)
        {
            return false;
        }

        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);
        return angleToTarget <= coneAngle * 0.5f;
    }

    private void CreateAreaVisual()
    {
        Transform existingVisual = transform.Find("SlowAreaVisual");
        GameObject visualObject;

        if (existingVisual != null)
        {
            visualObject = existingVisual.gameObject;
        }
        else
        {
            visualObject = new GameObject("SlowAreaVisual");
            visualObject.transform.SetParent(transform, false);
        }

        visualObject.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        visualObject.transform.localRotation = Quaternion.identity;

        areaMeshFilter = visualObject.GetComponent<MeshFilter>();
        if (areaMeshFilter == null)
        {
            areaMeshFilter = visualObject.AddComponent<MeshFilter>();
        }

        areaMeshRenderer = visualObject.GetComponent<MeshRenderer>();
        if (areaMeshRenderer == null)
        {
            areaMeshRenderer = visualObject.AddComponent<MeshRenderer>();
        }

        if (areaMesh == null)
        {
            areaMesh = new Mesh();
            areaMesh.name = "SlowAreaMesh";
        }

        areaMeshFilter.sharedMesh = areaMesh;

        if (areaMeshRenderer.sharedMaterial == null)
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
            Color slowColor = new Color(0.2f, 0.7f, 1f, 0.3f);
            material.color = slowColor;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", slowColor);
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
            areaMeshRenderer.sharedMaterial = material;
        }

        areaMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        areaMeshRenderer.receiveShadows = false;
    }

    private void UpdateAreaVisual()
    {
        if (areaMeshFilter == null || areaMesh == null)
        {
            return;
        }

        Vector3[] vertices = new Vector3[visualSegments + 2];
        int[] triangles = new int[visualSegments * 3];
        Vector2[] uvs = new Vector2[vertices.Length];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0f);

        float startAngle = -coneAngle * 0.5f;
        float angleStep = coneAngle / visualSegments;

        for (int i = 0; i <= visualSegments; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Vector3 point = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward * range;
            vertices[i + 1] = point;
            uvs[i + 1] = new Vector2((point.x / (range * 2f)) + 0.5f, point.z / range);
        }

        for (int i = 0; i < visualSegments; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        areaMesh.Clear();
        areaMesh.vertices = vertices;
        areaMesh.triangles = triangles;
        areaMesh.uv = uvs;
        areaMesh.RecalculateNormals();
        areaMesh.RecalculateBounds();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, range);

        Vector3 leftBoundary = Quaternion.Euler(0f, -coneAngle * 0.5f, 0f) * transform.forward * range;
        Vector3 rightBoundary = Quaternion.Euler(0f, coneAngle * 0.5f, 0f) * transform.forward * range;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}

