using System.Collections.Generic;
using UnityEngine;

public class FireTower : MonoBehaviour, ITower
{
    public float range = 6f;
    public float angle = 90f;
    public float damagePerSecond = 90f;
    public int visualSegments = 24;

    private MeshFilter areaMeshFilter;
    private MeshRenderer areaMeshRenderer;
    private Mesh areaMesh;
    private bool statsCached;
    private float baseRange;
    private float baseDamagePerSecond;
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
        angle = Mathf.Clamp(angle, 1f, 180f);
        range = Mathf.Max(0.1f, range);
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
        ApplyAreaDamage();
    }

    public bool CanUpgrade() => level < 3;

    public int GetUpgradeCost()
    {
        if (level == 1) return 60;
        if (level == 2) return 110;
        return 0;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.SpendMoney(GetUpgradeCost())) return false;

        level++;
        ApplyLevelStats();
        UpdateAreaVisual();
        return true;
    }

    private void CacheBaseStats()
    {
        if (statsCached) return;

        baseRange = range;
        baseDamagePerSecond = damagePerSecond;
        statsCached = true;
    }

    private void ApplyLevelStats()
    {
        int index = Mathf.Clamp(level - 1, 0, 2);
        float[] rangeMultipliers = { 1f, 1.1f, 1.22f };
        float[] damageMultipliers = { 1f, 1.45f, 2.15f };

        range = baseRange * rangeMultipliers[index];
        damagePerSecond = baseDamagePerSecond * damageMultipliers[index];
    }

    private void ApplyAreaDamage()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth eh = enemies[i];
            if (eh == null || eh.IsDead) continue;
            if (!IsEnemyInFireArc(eh.transform.position)) continue;

            eh.TakeDamage(damagePerSecond * Time.deltaTime);
            eh.PlayFireFeedback();
        }
    }

    private bool IsEnemyInFireArc(Vector3 enemyPosition)
    {
        Vector3 directionToEnemy = enemyPosition - transform.position;
        directionToEnemy.y = 0f;

        if (directionToEnemy.sqrMagnitude > range * range) return false;
        if (directionToEnemy.sqrMagnitude < 0.0001f) return true;

        float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy.normalized);
        return angleToEnemy <= angle * 0.5f;
    }

    private void CreateAreaVisual()
    {
        Transform existingVisual = transform.Find("FireAreaVisual");
        GameObject visualObject = existingVisual != null
            ? existingVisual.gameObject
            : new GameObject("FireAreaVisual");

        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        visualObject.transform.localRotation = Quaternion.identity;

        areaMeshFilter = visualObject.GetComponent<MeshFilter>() ?? visualObject.AddComponent<MeshFilter>();
        areaMeshRenderer = visualObject.GetComponent<MeshRenderer>() ?? visualObject.AddComponent<MeshRenderer>();

        if (areaMesh == null)
        {
            areaMesh = new Mesh { name = "FireAreaMesh" };
        }

        areaMeshFilter.sharedMesh = areaMesh;

        if (areaMeshRenderer.sharedMaterial == null)
            areaMeshRenderer.sharedMaterial = TowerVisualHelper.CreateTransparentMaterial(new Color(1f, 0.35f, 0.05f, 0.3f));

        areaMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        areaMeshRenderer.receiveShadows = false;
    }

    private void UpdateAreaVisual()
    {
        if (areaMeshFilter == null || areaMesh == null) return;

        Vector3[] vertices = new Vector3[visualSegments + 2];
        int[] triangles = new int[visualSegments * 3];
        Vector2[] uvs = new Vector2[vertices.Length];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0f);

        float startAngle = -angle * 0.5f;
        float angleStep = angle / visualSegments;

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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Vector3 leftBoundary = Quaternion.Euler(0f, -angle * 0.5f, 0f) * transform.forward * range;
        Vector3 rightBoundary = Quaternion.Euler(0f, angle * 0.5f, 0f) * transform.forward * range;

        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
    }
}
