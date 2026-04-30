using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 25f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
            HitTarget();
    }

    private void HitTarget()
    {
        if (target != null)
        {
            EnemyHealth health = target.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        else
            Destroy(gameObject);
    }
}
