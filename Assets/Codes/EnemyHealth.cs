using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public int moneyReward = 10;

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }

        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.EnemyRemoved();
        }

        Destroy(gameObject);
    }
}