using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerTower : MonoBehaviour
{
    public float range = 6f;
    public float damagePerSecond = 45f;
    public float zoneWidth = 6f;
    public float zoneLength = 5.5f;
    public float zoneHeight = 1.2f;

    private FlameZone flameZone;
    private Renderer zoneRenderer;

    private void Awake()
    {
        CreateFlameZone();
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

    private GameObject FindNearestEnemy()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;

        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (EnemyHealth enemyHealth in enemies)
        {
            if (enemyHealth == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearest = enemyHealth.gameObject;
            }
        }

        return nearest;
    }

    private void RotateToward(Transform target)
    {
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
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
        if (visualCollider != null)
        {
            Destroy(visualCollider);
        }

        zoneRenderer = visualObject.GetComponent<Renderer>();
        if (zoneRenderer != null)
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
            Color zoneColor = new Color(1f, 0.45f, 0.1f, 0.3f);

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", zoneColor);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", zoneColor);
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
            zoneRenderer.material = material;
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

    private void SetZoneActive(bool isActive)
    {
        if (flameZone == null)
        {
            return;
        }

        flameZone.damagePerSecond = damagePerSecond;
        flameZone.SetActiveState(isActive);

        if (zoneRenderer != null)
        {
            zoneRenderer.enabled = isActive;
        }
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
