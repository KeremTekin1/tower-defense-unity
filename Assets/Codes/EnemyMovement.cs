using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public int baseDamage = 1;

    private int currentWaypointIndex = 0;
    private bool reachedBase = false;
    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    public float GetPathProgress() => currentWaypointIndex;

    private void Update()
    {
        TickSlowEffect();

        if (waypoints == null || waypoints.Length == 0)
            return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachBase();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        float currentSpeed = speed * slowMultiplier;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
        );

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, target.position) < 0.15f)
            currentWaypointIndex++;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.2f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    private void TickSlowEffect()
    {
        if (slowTimer <= 0f)
        {
            slowMultiplier = 1f;
            return;
        }

        slowTimer -= Time.deltaTime;
        if (slowTimer <= 0f)
        {
            slowTimer = 0f;
            slowMultiplier = 1f;
        }
    }

    private void ReachBase()
    {
        if (reachedBase) return;

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.IsDead) return;

        reachedBase = true;
        WaveSpawner.ActiveEnemies.Remove(GetComponent<EnemyHealth>());

        if (GameManager.Instance != null)
            GameManager.Instance.DamageBase(baseDamage);

        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.EnemyRemoved();

        Destroy(gameObject);
    }
}
