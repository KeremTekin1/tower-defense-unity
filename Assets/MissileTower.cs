using UnityEngine;

public class MissileTower : MonoBehaviour
{
    public float range = 10f;
    public float fireRate = 1f;
    public GameObject missilePrefab;
    public Transform firePoint;

    private float fireCooldown = 0f;

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        GameObject target = FindNearestEnemy();
        if (target == null) return;

        Vector3 lookPos = target.transform.position - transform.position;
        lookPos.y = 0f;

        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookPos);
        }

        if (fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    GameObject FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (EnemyHealth enemyHealth in enemies)
        {
            if (enemyHealth == null) continue;

            float dist = Vector3.Distance(transform.position, enemyHealth.transform.position);

            if (dist < shortestDistance && dist <= range)
            {
                shortestDistance = dist;
                nearest = enemyHealth.gameObject;
            }
        }

        return nearest;
    }

    void Shoot(GameObject target)
    {
        if (missilePrefab == null || firePoint == null) return;

        GameObject missileObj = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
        MissileProjectile missile = missileObj.GetComponent<MissileProjectile>();

        if (missile != null)
        {
            missile.SetTarget(target);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}