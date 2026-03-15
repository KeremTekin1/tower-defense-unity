using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    private bool isDead = false;

    private void Start()
    {
        SetHealthToMax();
    }

    public void SetHealthToMax()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        WaveSpawner.enemiesKilled++;
        Destroy(gameObject);
    }
}