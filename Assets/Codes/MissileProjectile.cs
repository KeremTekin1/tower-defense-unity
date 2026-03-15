using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 80f;
    public float lifeTime = 4f;

    private Vector3 direction;
    private bool isLaunched = false;
    private bool hasHit = false;
    private float lifetimeTimer = 0f;

    private void OnEnable()
    {
        // Reset state every time pulled from pool
        direction = Vector3.zero;
        isLaunched = false;
        hasHit = false;
        lifetimeTimer = 0f;
    }

    public void SetTarget(GameObject target)
    {
        if (target == null)
        {
            ReturnToPool();
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
        lifetimeTimer = lifeTime;
    }

    private void Update()
    {
        if (!isLaunched || hasHit) return;

        transform.position += direction * speed * Time.deltaTime;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();

        if (health != null)
        {
            hasHit = true;
            health.TakeDamage(damage);
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnMissile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}