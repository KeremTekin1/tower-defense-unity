using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 140f;
    public float lifeTime = 4f;

    private Transform target;
    private Vector3 direction;
    private bool useHoming;
    private bool isLaunched;
    private bool hasHit;
    private float lifetimeTimer;

    private void OnEnable()
    {
        target = null;
        direction = Vector3.zero;
        useHoming = false;
        isLaunched = false;
        hasHit = false;
        lifetimeTimer = 0f;
    }

    public void SetTarget(GameObject newTarget, bool homing)
    {
        if (newTarget == null)
        {
            ReturnToPool();
            return;
        }

        useHoming = homing;
        target = newTarget.transform;

        if (useHoming)
        {
            isLaunched = true;
            lifetimeTimer = lifeTime;
            return;
        }

        EnemyMovement enemyMove = newTarget.GetComponent<EnemyMovement>();
        Vector3 targetPos = newTarget.transform.position;
        Vector3 predictedPos = targetPos;

        if (enemyMove != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPos);
            float timeToReach = distanceToTarget / speed;
            Vector3 enemyForward = newTarget.transform.forward;
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
        if (!isLaunched || hasHit)
        {
            return;
        }

        if (useHoming)
        {
            UpdateHomingMovement();
        }
        else
        {
            UpdatePredictiveMovement();
        }

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void UpdateHomingMovement()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget(target.GetComponent<EnemyHealth>());
        }
    }

    private void UpdatePredictiveMovement()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
        {
            return;
        }

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();
        if (health != null)
        {
            HitTarget(health);
        }
    }

    private void HitTarget(EnemyHealth health)
    {
        if (hasHit)
        {
            return;
        }

        hasHit = true;

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
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
