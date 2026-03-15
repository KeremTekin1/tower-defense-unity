using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float fireCooldown;

    private void Update()
    {
        TickCooldown();

        GameObject target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        RotateToward(target.transform);
        TryShoot(target.transform);
    }

    private void TickCooldown()
    {
        fireCooldown -= Time.deltaTime;
    }

    private GameObject FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

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

    private void TryShoot(Transform target)
    {
        if (fireCooldown > 0f)
        {
            return;
        }

        Shoot(target);
        fireCooldown = 1f / fireRate;
    }

    private void Shoot(Transform target)
    {
        if (firePoint == null)
        {
            Debug.LogError("Tower: firePoint atanmadi.");
            return;
        }

        if (ProjectilePool.Instance == null)
        {
            Debug.LogError("Tower: ProjectilePool sahnede bulunamadi.");
            return;
        }

        GameObject projectileObject = ProjectilePool.Instance.GetProjectile();
        if (projectileObject == null)
        {
            Debug.LogError("Tower: Pool'dan projectile alinamadi.");
            return;
        }

        projectileObject.transform.position = firePoint.position;
        projectileObject.transform.rotation = Quaternion.identity;

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null)
        {
            Debug.LogError("Projectile objesinde Projectile scripti yok.");
            return;
        }

        projectile.SetTarget(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
