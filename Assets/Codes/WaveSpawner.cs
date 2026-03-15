using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;

    [Header("Wave Settings")]
    public float spawnInterval = 1.2f;
    public float timeBetweenWaves = 5f;
    public int startEnemiesPerWave = 5;
    public int enemyIncreasePerWave = 3;
    public int maxWavesLevel1 = 3;

    [HideInInspector] public int currentWave = 0;
    [HideInInspector] public float nextWaveCountdown = 0f;

    public static int enemiesKilled = 0;

    private int aliveEnemies = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        enemiesKilled = 0;
        aliveEnemies = 0;

        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner: enemyPrefab atanmadý.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("WaveSpawner: spawnPoint atanmadý.");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("WaveSpawner: waypoints boþ.");
            return;
        }

        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (true)
        {
            currentWave++;
            nextWaveCountdown = 0f;

            int enemiesThisWave = startEnemiesPerWave + (currentWave - 1) * enemyIncreasePerWave;
            Debug.Log("Wave baþladý: " + currentWave + " | Enemy sayýsý: " + enemiesThisWave);

            for (int i = 0; i < enemiesThisWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // Bu wave'deki tüm enemy'ler bitene kadar bekle
            while (aliveEnemies > 0)
            {
                yield return null;
            }

            // level1 tamamlanýnca level2'ye geç
            if (SceneManager.GetActiveScene().name == "level1" && currentWave >= maxWavesLevel1)
            {
                Debug.Log("Level 1 tamamlandý. level2 yükleniyor...");
                SceneManager.LoadScene("level2");
                yield break;
            }

            // Sonraki wave geri sayýmý
            nextWaveCountdown = timeBetweenWaves;

            while (nextWaveCountdown > 0f)
            {
                nextWaveCountdown -= Time.deltaTime;
                yield return null;
            }

            nextWaveCountdown = 0f;
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        if (enemy == null)
        {
            Debug.LogError("WaveSpawner: Enemy oluþturulamadý.");
            return;
        }

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        Renderer rend = enemy.GetComponentInChildren<Renderer>();

        if (movement == null)
        {
            Debug.LogError("Enemy prefabýnda EnemyMovement yok.");
            Destroy(enemy);
            return;
        }

        if (health == null)
        {
            Debug.LogError("Enemy prefabýnda EnemyHealth yok.");
            Destroy(enemy);
            return;
        }

        movement.waypoints = waypoints;
        aliveEnemies++;

        int rand = Random.Range(0, 3);

        if (rand == 0)
        {
            if (rend != null) rend.material.color = Color.yellow;
            movement.speed = 9f;
            health.maxHealth = 180f;
            health.moneyReward = 8;
        }
        else if (rand == 1)
        {
            if (rend != null) rend.material.color = Color.black;
            movement.speed = 3f;
            health.maxHealth = 600f;
            health.moneyReward = 20;
        }
        else
        {
            if (rend != null) rend.material.color = Color.red;
            movement.speed = 5.5f;
            health.maxHealth = 300f;
            health.moneyReward = 12;
        }

        health.SetHealthToMax();
    }

    public void EnemyRemoved()
    {
        aliveEnemies--;
        if (aliveEnemies < 0) aliveEnemies = 0;

        Debug.Log("Kalan enemy: " + aliveEnemies);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}