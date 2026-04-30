using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public GameObject enemyPrefab;
    public EnemyData[] enemyTypes;
    public Transform spawnPoint;
    public Transform[] waypoints;

    [Header("Wave Settings")]
    public float spawnInterval = 1.2f;
    public float timeBetweenWaves = 5f;
    public int startEnemiesPerWave = 5;
    public int enemyIncreasePerWave = 3;
    public int maxWavesLevel1 = 3;
    public string nextSceneName = "";

    [HideInInspector] public int currentWave = 0;
    [HideInInspector] public float nextWaveCountdown = 0f;

    public static int enemiesKilled = 0;
    public static readonly List<EnemyHealth> ActiveEnemies = new List<EnemyHealth>();

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
        ActiveEnemies.Clear();
        aliveEnemies = 0;

        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner: enemyPrefab not assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("WaveSpawner: spawnPoint not assigned.");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("WaveSpawner: waypoints array is empty.");
            return;
        }

        nextWaveEnemyTypes = GenerateWaveEnemyTypes(1);
        StartCoroutine(WaveRoutine());
    }

    private void Update()
    {
        if (nextWaveCountdown > 0.1f && Input.GetKeyDown(KeyCode.Space))
            nextWaveCountdown = 0f;
    }

    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            currentWave++;
            nextWaveCountdown = 0f;

            int[] currentWaveEnemyTypes = nextWaveEnemyTypes;
            nextWaveEnemyTypes = GenerateWaveEnemyTypes(currentWave + 1);

            for (int i = 0; i < currentWaveEnemyTypes.Length; i++)
            {
                SpawnEnemy(currentWaveEnemyTypes[i]);
                yield return new WaitForSeconds(spawnInterval);
            }

            while (aliveEnemies > 0)
                yield return null;

            if (WavePanelUI.Instance != null)
                WavePanelUI.Instance.ShowWaveComplete(1.8f);

            if (currentWave >= maxWavesLevel1)
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
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
        int[] generatedEnemyTypes = new int[enemyCount];

        for (int i = 0; i < enemyCount; i++)
            generatedEnemyTypes[i] = Random.Range(0, 3);

        if (waveNumber >= 3 && enemyCount > 0 && waveNumber % 3 == 0)
            generatedEnemyTypes[enemyCount - 1] = 3;

        return generatedEnemyTypes;
    }

    private int GetEnemyCountForWave(int waveNumber)
    {
        return startEnemiesPerWave + (waveNumber - 1) * enemyIncreasePerWave;
    }

    private void SpawnEnemy(int enemyType)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        Renderer rend = enemy.GetComponentInChildren<Renderer>();

        if (movement == null)
        {
            Debug.LogError("Enemy prefab is missing EnemyMovement component.");
            Destroy(enemy);
            return;
        }

        if (health == null)
        {
            Debug.LogError("Enemy prefab is missing EnemyHealth component.");
            Destroy(enemy);
            return;
        }

        movement.waypoints = waypoints;
        movement.baseDamage = 1;
        enemy.transform.localScale = Vector3.one;
        aliveEnemies++;
        ActiveEnemies.Add(health);

        bool hasScriptableData = enemyTypes != null && enemyType >= 0 && enemyType < enemyTypes.Length;
        EnemyData data = hasScriptableData ? enemyTypes[enemyType] : null;

        if (data != null)
        {
            movement.speed = data.speed;
            health.maxHealth = data.maxHealth;
            health.moneyReward = data.moneyReward;
            if (rend != null) rend.material.color = data.color;
        }
        else
        {
            ApplyHardcodedEnemyType(enemyType, movement, health, rend, enemy);
        }

        float waveScale = 1f + (currentWave - 1) * 0.12f;
        health.maxHealth *= waveScale;
        movement.speed = Mathf.Min(movement.speed * (1f + (currentWave - 1) * 0.05f), 18f);

        health.SetHealthToMax();
    }

    private void ApplyHardcodedEnemyType(int enemyType, EnemyMovement movement, EnemyHealth health, Renderer rend, GameObject enemy)
    {
        switch (enemyType)
        {
            case 0:
                if (rend != null) rend.material.color = Color.yellow;
                movement.speed = 9f;
                health.maxHealth = 180f;
                health.moneyReward = 8;
                break;
            case 1:
                if (rend != null) rend.material.color = Color.black;
                movement.speed = 3f;
                health.maxHealth = 600f;
                health.moneyReward = 20;
                break;
            case 2:
                if (rend != null) rend.material.color = Color.red;
                movement.speed = 5.5f;
                health.maxHealth = 300f;
                health.moneyReward = 12;
                break;
            default:
                if (rend != null) rend.material.color = new Color(0.82f, 0.35f, 1f);
                movement.speed = 2.4f;
                movement.baseDamage = 8;
                health.maxHealth = 2600f;
                health.moneyReward = 60;
                enemy.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
                break;
        }
    }

    public string GetNextWavePreviewText()
    {
        if (nextWaveEnemyTypes == null || nextWaveEnemyTypes.Length == 0)
            return "Preparing next wave...";

        int fastCount = 0, tankCount = 0, basicCount = 0, godCount = 0;

        for (int i = 0; i < nextWaveEnemyTypes.Length; i++)
        {
            switch (nextWaveEnemyTypes[i])
            {
                case 0: fastCount++; break;
                case 1: tankCount++; break;
                case 2: basicCount++; break;
                default: godCount++; break;
            }
        }

        return $"FAST  {fastCount}\nTANK  {tankCount}\nBASIC  {basicCount}\nGOD  {godCount}";
    }

    public void EnemyRemoved()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
