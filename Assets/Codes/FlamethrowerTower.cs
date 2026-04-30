using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerTower : MonoBehaviour, ITower
{
    public float range = 6f;
    public float damagePerSecond = 45f;
    public float zoneWidth = 6f;
    public float zoneLength = 5.5f;
    public float zoneHeight = 1.2f;

    private FlameZone flameZone;
    private Renderer zoneRenderer;
    private bool statsCached;
    private float baseRange;
    private float baseDamagePerSecond;
    private float baseZoneLength;
    private int level = 1;

    public int Level => level;

    private void Awake()
    {
        CacheBaseStats();
        CreateFlameZone();
        ApplyLevelStats();
    }

    private void Update()
    {
        GameObject target = FindNearestEnemy();
        if (target == null)
        {
            SetZoneActive(false);
            return;
        }

        RotateToward(target.transform);
        SetZoneActive(true);
    }

    public bool CanUpgrade() => level < 3;

    public int GetUpgradeCost()
    {
        if (level == 1) return 55;
        if (level == 2) return 95;
        return 0;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.SpendMoney(GetUpgradeCost())) return false;

        level++;
        ApplyLevelStats();
        UpdateFlameZoneSize();
        return true;
    }

    private void CacheBaseStats()
    {
        if (statsCached) return;

        baseRange = range;
        baseDamagePerSecond = damagePerSecond;
        baseZoneLength = zoneLength;
        statsCached = true;
    }

    private void ApplyLevelStats()
    {
        float[] rangeMultipliers = { 1f, 1.1f, 1.22f };
        float[] damageMultipliers = { 1f, 1.5f, 2.2f };
        float[] zoneLengthMultipliers = { 1f, 1.2f, 1.45f };

        int index = Mathf.Clamp(level - 1, 0, 2);
        range = baseRange * rangeMultipliers[index];
        damagePerSecond = baseDamagePerSecond * damageMultipliers[index];
        zoneLength = baseZoneLength * zoneLengthMultipliers[index];
    }

    private GameObject FindNearestEnemy()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth eh = enemies[i];
            if (eh == null || eh.IsDead) continue;

            float distance = Vector3.Distance(transform.position, eh.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearest = eh.gameObject;
            }
        }

        return nearest;
    }

    private void RotateToward(Transform target)
    {
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void CreateFlameZone()
    {
        GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visualObject.name = "FlameZoneVisual";
        visualObject.transform.SetParent(transform);
        visualObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        visualObject.transform.localPosition = new Vector3(0f, 0.02f, zoneLength * 0.5f);
        visualObject.transform.localScale = new Vector3(zoneWidth, zoneLength, 1f);

        Collider visualCollider = visualObject.GetComponent<Collider>();
        if (visualCollider != null) Destroy(visualCollider);

        zoneRenderer = visualObject.GetComponent<Renderer>();
        if (zoneRenderer != null)
        {
            zoneRenderer.material = TowerVisualHelper.CreateTransparentMaterial(new Color(1f, 0.45f, 0.1f, 0.3f));
            zoneRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            zoneRenderer.receiveShadows = false;
            zoneRenderer.enabled = false;
        }

        GameObject triggerObject = new GameObject("FlameZoneTrigger");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localRotation = Quaternion.identity;
        triggerObject.transform.localPosition = new Vector3(0f, zoneHeight * 0.5f, zoneLength * 0.5f);

        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(zoneWidth, zoneHeight, zoneLength);

        Rigidbody zoneBody = triggerObject.AddComponent<Rigidbody>();
        zoneBody.useGravity = false;
        zoneBody.isKinematic = true;

        flameZone = triggerObject.AddComponent<FlameZone>();
        flameZone.damagePerSecond = damagePerSecond;
        flameZone.SetActiveState(false);
    }

    private void UpdateFlameZoneSize()
    {
        if (flameZone == null) return;

        Transform visual = transform.Find("FlameZoneVisual");
        if (visual != null)
        {
            visual.localPosition = new Vector3(0f, 0.02f, zoneLength * 0.5f);
            visual.localScale = new Vector3(zoneWidth, zoneLength, 1f);
        }

        Transform trigger = transform.Find("FlameZoneTrigger");
        if (trigger != null)
        {
            trigger.localPosition = new Vector3(0f, zoneHeight * 0.5f, zoneLength * 0.5f);
            BoxCollider bc = trigger.GetComponent<BoxCollider>();
            if (bc != null) bc.size = new Vector3(zoneWidth, zoneHeight, zoneLength);
        }
    }

    private void SetZoneActive(bool isActive)
    {
        if (flameZone == null) return;

        flameZone.damagePerSecond = damagePerSecond;
        flameZone.SetActiveState(isActive);

        if (zoneRenderer != null)
            zoneRenderer.enabled = isActive;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.6f);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + transform.forward * (zoneLength * 0.5f) + Vector3.up * (zoneHeight * 0.5f),
            transform.rotation,
            new Vector3(zoneWidth, zoneHeight, zoneLength)
        );
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = oldMatrix;
    }
}
