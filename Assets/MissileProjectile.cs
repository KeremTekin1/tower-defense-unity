using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 80f;
    public float lifeTime = 4f;

    private Vector3 direction;
    private bool isLaunched = false;
    private bool hasHit = false;

    public void SetTarget(GameObject target)
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        EnemyMovement enemyMove = target.GetComponent<EnemyMovement>();

        Vector3 targetPos = target.transform.position;
        Vector3 predictedPos = targetPos;

        if (enemyMove != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPos);
            float timeToReach = distanceToTarget / speed;

            Vector3 enemyForward = target.transform.forward;
            float enemySpeed = enemyMove.speed;

            predictedPos = targetPos + enemyForward * enemySpeed * timeToReach;
        }

        direction = (predictedPos - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        isLaunched = true;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!isLaunched || hasHit) return;

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();

        if (health != null)
        {
            hasHit = true;
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}