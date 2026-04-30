using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, ITower
{
    public float range = 8f;
    public float fireRate = 1f;
    public float damage = 25f;
    public TargetMode targetMode = TargetMode.Nearest;
    public GameObject projectilePrefab;
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

        GameObject target = FindTarget();
        if (target == null) return;

        RotateToward(target.transform);
        TryShoot(target.transform);
    }

    public bool CanUpgrade() => level < 3;

    public int GetUpgradeCost()
    {
        if (level == 1) return 45;
        if (level == 2) return 85;
        return 0;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.SpendMoney(GetUpgradeCost())) return false;

        level++;
        ApplyLevelStats();
        return true;
    }

    private void CacheBaseStats()
    {
        if (statsCached) return;

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
        statsCached = true;
    }

    private void ApplyLevelStats()
    {
        float[] rangeMultipliers = { 1f, 1.12f, 1.26f };
        float[] fireRateMultipliers = { 1f, 1.22f, 1.48f };
        float[] damageMultipliers = { 1f, 1.65f, 2.4f };

        int index = Mathf.Clamp(level - 1, 0, 2);
        range = baseRange * rangeMultipliers[index];
        fireRate = baseFireRate * fireRateMultipliers[index];
        damage = baseDamage * damageMultipliers[index];
    }

    private void TickCooldown()
    {
        fireCooldown -= Time.deltaTime;
    }

    private GameObject FindTarget()
    {
        List<EnemyHealth> enemies = WaveSpawner.ActiveEnemies;
        GameObject best = null;
        float bestValue = targetMode == TargetMode.Nearest ? Mathf.Infinity : -Mathf.Infinity;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyHealth eh = enemies[i];
            if (eh == null || eh.IsDead) continue;

            float dist = Vector3.Distance(transform.position, eh.transform.position);
            if (dist > range) continue;

            switch (targetMode)
            {
                case TargetMode.First:
                    float progress = eh.Movement != null ? eh.Movement.GetPathProgress() : 0f;
                    if (progress > bestValue) { bestValue = progress; best = eh.gameObject; }
                    break;
                case TargetMode.Strongest:
                    if (eh.currentHealth > bestValue) { bestValue = eh.currentHealth; best = eh.gameObject; }
                    break;
                default:
                    if (dist < bestValue) { bestValue = dist; best = eh.gameObject; }
                    break;
            }
        }

        return best;
    }

    private void RotateToward(Transform target)
    {
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void TryShoot(Transform target)
    {
        if (fireCooldown > 0f) return;

        Shoot(target);
        fireCooldown = 1f / fireRate;
    }

    private void Shoot(Transform target)
    {
        if (firePoint == null)
        {
            Debug.LogError("Tower: firePoint not assigned.");
            return;
        }

        if (ProjectilePool.Instance == null)
        {
            Debug.LogError("Tower: ProjectilePool not found in scene.");
            return;
        }

        GameObject projectileObject = ProjectilePool.Instance.GetProjectile();
        if (projectileObject == null)
        {
            Debug.LogError("Tower: Failed to get projectile from pool.");
            return;
        }

        projectileObject.transform.position = firePoint.position;
        projectileObject.transform.rotation = Quaternion.identity;

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null)
        {
            Debug.LogError("Projectile object is missing the Projectile script.");
            return;
        }

        projectile.damage = damage;
        projectile.SetTarget(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
