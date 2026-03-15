using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Projectile Pool")]
    public GameObject projectilePrefab;
    public int projectilePoolSize = 30;

    [Header("Missile Pool")]
    public GameObject missilePrefab;
    public int missilePoolSize = 15;

    private readonly Queue<GameObject> projectilePool = new Queue<GameObject>();
    private readonly Queue<GameObject> missilePool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool(projectilePrefab, projectilePool, projectilePoolSize);
        InitializePool(missilePrefab, missilePool, missilePoolSize);
    }

    public GameObject GetProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("ProjectilePool: projectile prefab atanmadi.");
            return null;
        }

        return GetFromPool(projectilePool, projectilePrefab);
    }

    public void ReturnProjectile(GameObject obj)
    {
        ReturnToPool(obj, projectilePool);
    }

    public GameObject GetMissile()
    {
        if (missilePrefab == null)
        {
            Debug.LogError("ProjectilePool: missile prefab atanmadi.");
            return null;
        }

        return GetFromPool(missilePool, missilePrefab);
    }

    public void ReturnMissile(GameObject obj)
    {
        ReturnToPool(obj, missilePool);
    }

    private void InitializePool(GameObject prefab, Queue<GameObject> pool, int size)
    {
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj == null)
            {
                continue;
            }

            obj.transform.SetParent(null);
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"[ProjectilePool] Pool for {prefab.name} exhausted, instantiating one extra.");
        return Instantiate(prefab);
    }

    private void ReturnToPool(GameObject obj, Queue<GameObject> pool)
    {
        if (obj == null)
        {
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }
}
