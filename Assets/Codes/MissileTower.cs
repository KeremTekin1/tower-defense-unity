using System.Collections.Generic;
using UnityEngine;
 
public class MissileTower : MonoBehaviour, ITower
{
    public float range = 10f;
    public float fireRate = 1f;
    public float damage = 140f;
    public bool useHomingMissile = true;
    public GameObject missilePrefab;
    public Transform firePoint;
 
    private float fireCooldown;
    private bool statsCached;
    private float baseRange;
    private float baseFireRate;
    private float baseDamage;
    private int level = 1;

    public int Level => level;
 
    private void Awake()
    {
        CacheBaseStats();
        ApplyLevelStats();
    }

    private void Update()
    {
        TickCooldown();
 
        GameObject target = FindNearestEnemy();
        if (target == null) return;
 
        RotateToward(target.transform);
        TryShoot(target);
    }

    public bool CanUpgrade()
    {
        return level < 3;
    }

    public int GetUpgradeCost()
    {
        if (level == 1)
        {
            return useHomingMissile ? 70 : 60;
        }

        if (level == 2)
        {
            return useHomingMissile ? 120 : 100;
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
        baseDamage = damage;
        statsCached = true;
    }

    private void ApplyLevelStats()
    {
        float[] rangeMultipliers = { 1f, 1.14f, 1.28f };
        float[] fireRateMultipliers = useHomingMissile
            ? new float[] { 1f, 1.16f, 1.32f }
            : new float[] { 1f, 1.2f, 1.38f };
        float[] damageMultipliers = useHomingMissile
            ? new float[] { 1f, 1.45f, 2f }
            : new float[] { 1f, 1.5f, 2.1f };

        int index = Mathf.Clamp(level - 1, 0, 2);
        range = baseRange * rangeMultipliers[index];
        fireRate = baseFireRate * fireRateMultipliers[index];
        damage = baseDamage * damageMultipliers[index];
    }
 
    private void TickCooldown()
    {
        fireCooldown -= Time.deltaTime;
    }
 
    private GameObject FindNearestEnemy()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;
 
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;
 
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth enemyHealth = enemies[i];
            if (enemyHealth == null || enemyHealth.IsDead) continue;
 
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
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }
 
    private void TryShoot(GameObject target)
    {
        if (fireCooldown > 0f) return;
 
        if (Shoot(target))
            fireCooldown = 1f / fireRate;
    }
 
    private bool Shoot(GameObject target)
    {
        if (target == null) return false;
 
        if (firePoint == null)
        {
            Debug.LogError("[MissileTower] firePoint atanmadý!");
            return false;
        }
 
        if (missilePrefab == null)
        {
            Debug.LogError("[MissileTower] missilePrefab atanmadý! Inspector'dan ata.");
            return false;
        }
 
        GameObject missileObject = null;
 
        if (ProjectilePool.Instance != null)
            missileObject = ProjectilePool.Instance.GetMissile();
 
        if (missileObject == null)
            missileObject = Instantiate(missilePrefab);
 
        missileObject.transform.position = firePoint.position;
        missileObject.transform.rotation = Quaternion.identity;
 
        MissileProjectile missile = missileObject.GetComponent<MissileProjectile>();
 
        if (missile == null)
        {
            Debug.LogError("[MissileTower] Prefabda MissileProjectile scripti yok!");
            Destroy(missileObject);
            return false;
        }
 
        missile.damage = damage;
        missile.SetTarget(target, useHomingMissile);
        return true;
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

