using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;

    public float spawnInterval = 1.2f;

    public static int enemiesKilled = 0;

    private void Start()
    {
        enemiesKilled = 0;

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

        Debug.Log("WaveSpawner baþladý. Aktif sahne: " + SceneManager.GetActiveScene().name);
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();

            if (enemiesKilled >= 10 && SceneManager.GetActiveScene().name == "level1")
            {
                Debug.Log("10 düþman öldürüldü. level2 yükleniyor...");
                SceneManager.LoadScene("level2");
                yield break;
            }

            yield return new WaitForSeconds(spawnInterval);
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

        // 3 farklý enemy tipi
        int rand = Random.Range(0, 3);

        if (rand == 0)
        {
            // Hýzlý düþman
            if (rend != null) rend.material.color = Color.yellow;
            movement.speed = 9f;
            health.maxHealth = 180f;
        }
        else if (rand == 1)
        {
            // Tank düþman
            if (rend != null) rend.material.color = Color.black;
            movement.speed = 3f;
            health.maxHealth = 600f;
        }
        else
        {
            // Normal düþman
            if (rend != null) rend.material.color = Color.red;
            movement.speed = 5.5f;
            health.maxHealth = 300f;
        }

        health.SetHealthToMax();
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