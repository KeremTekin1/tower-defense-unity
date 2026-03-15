using UnityEngine;

public class MissileTower : MonoBehaviour
{
    public float range = 10f;
    public float fireRate = 1f;
    public GameObject missilePrefab;
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
        TryShoot(target);
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

    private void TryShoot(GameObject target)
    {
        if (fireCooldown > 0f)
        {
            return;
        }

        Shoot(target);
        fireCooldown = 1f / fireRate;
    }

    private void Shoot(GameObject target)
    {
        if (firePoint == null || ProjectilePool.Instance == null)
        {
            return;
        }

        GameObject missileObject = ProjectilePool.Instance.GetMissile();
        if (missileObject == null)
        {
            return;
        }

        missileObject.transform.position = firePoint.position;
        missileObject.transform.rotation = Quaternion.identity;

        MissileProjectile missile = missileObject.GetComponent<MissileProjectile>();
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
