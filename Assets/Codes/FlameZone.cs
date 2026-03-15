using UnityEngine;

public class FlameZone : MonoBehaviour
{
    public float damagePerSecond = 45f;
    public bool isActive;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        if (zoneCollider != null)
        {
            zoneCollider.enabled = active;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive)
        {
            return;
        }

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();
        if (health == null)
        {
            return;
        }

        health.TakeDamage(damagePerSecond * Time.deltaTime);
    }
}
