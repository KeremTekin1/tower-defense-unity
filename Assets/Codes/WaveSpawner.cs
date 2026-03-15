using System.Collections;
using UnityEngine;
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
    private int[] nextWaveEnemyTypes;

    public int AliveEnemies => aliveEnemies;

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
            Debug.LogError("WaveSpawner: enemyPrefab atanmadı.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("WaveSpawner: spawnPoint atanmadı.");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("WaveSpawner: waypoints boş.");
            return;
        }

        nextWaveEnemyTypes = GenerateWaveEnemyTypes(1);
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            currentWave++;
            nextWaveCountdown = 0f;

            int[] currentWaveEnemyTypes = nextWaveEnemyTypes;
            nextWaveEnemyTypes = GenerateWaveEnemyTypes(currentWave + 1);

            Debug.Log("Wave başladı: " + currentWave + " | Enemy sayısı: " + currentWaveEnemyTypes.Length);

            for (int i = 0; i < currentWaveEnemyTypes.Length; i++)
            {
                SpawnEnemy(currentWaveEnemyTypes[i]);
                yield return new WaitForSeconds(spawnInterval);
            }

            while (aliveEnemies > 0)
            {
                yield return null;
            }

            if (SceneManager.GetActiveScene().name == "level1" && currentWave >= maxWavesLevel1)
            {
                Debug.Log("Level 1 tamamlandı. level2 yükleniyor...");
                SceneManager.LoadScene("level2");
                yield break;
            }

            nextWaveCountdown = timeBetweenWaves;

            while (nextWaveCountdown > 0f)
            {
                nextWaveCountdown -= Time.deltaTime;
                yield return null;
            }

            nextWaveCountdown = 0f;
        }
    }

    private int[] GenerateWaveEnemyTypes(int waveNumber)
    {
        int enemyCount = GetEnemyCountForWave(waveNumber);
        int[] enemyTypes = new int[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            enemyTypes[i] = Random.Range(0, 3);
        }

        return enemyTypes;
    }

    private int GetEnemyCountForWave(int waveNumber)
    {
        return startEnemiesPerWave + (waveNumber - 1) * enemyIncreasePerWave;
    }

    private void SpawnEnemy(int enemyType)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        if (enemy == null)
        {
            Debug.LogError("WaveSpawner: Enemy oluşturulamadı.");
            return;
        }

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        Renderer rend = enemy.GetComponentInChildren<Renderer>();

        if (movement == null)
        {
            Debug.LogError("Enemy prefabında EnemyMovement yok.");
            Destroy(enemy);
            return;
        }

        if (health == null)
        {
            Debug.LogError("Enemy prefabında EnemyHealth yok.");
            Destroy(enemy);
            return;
        }

        movement.waypoints = waypoints;
        aliveEnemies++;

        if (enemyType == 0)
        {
            if (rend != null) rend.material.color = Color.yellow;
            movement.speed = 9f;
            health.maxHealth = 180f;
            health.moneyReward = 8;
        }
        else if (enemyType == 1)
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

    public string GetNextWavePreviewText()
    {
        if (nextWaveEnemyTypes == null || nextWaveEnemyTypes.Length == 0)
        {
            return "Preparing next wave...";
        }

        int fastCount = 0;
        int tankCount = 0;
        int basicCount = 0;

        for (int i = 0; i < nextWaveEnemyTypes.Length; i++)
        {
            if (nextWaveEnemyTypes[i] == 0)
            {
                fastCount++;
            }
            else if (nextWaveEnemyTypes[i] == 1)
            {
                tankCount++;
            }
            else
            {
                basicCount++;
            }
        }

        return "FAST  " + fastCount + "\nTANK  " + tankCount + "\nBASIC  " + basicCount;
    }

    public void EnemyRemoved()
    {
        aliveEnemies--;
        if (aliveEnemies < 0)
        {
            aliveEnemies = 0;
        }

        Debug.Log("Kalan enemy: " + aliveEnemies);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }

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
