using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;

    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachBase();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.15f)
        {
            currentWaypointIndex++;
        }
    }

    void ReachBase()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DamageBase(1);
        }

        Destroy(gameObject);
    }
}