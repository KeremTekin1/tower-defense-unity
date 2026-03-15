using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float fireCooldown = 0f;

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        GameObject target = FindNearestEnemy();

        if (target == null)
            return;

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
        if (projectilePrefab == null)
        {
            Debug.LogError("Tower: projectilePrefab atanmadý.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Tower: firePoint atanmadý.");
            return;
        }

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetTarget(target.transform);
        }
        else
        {
            Debug.LogError("Projectile prefabýnda Projectile scripti yok.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}