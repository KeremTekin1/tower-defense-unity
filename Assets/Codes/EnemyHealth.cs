using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public int moneyReward = 10;

    private bool isDead = false;
    private EnemyStatusVfx statusVfx;

    public float HealthNormalized
    {
        get
        {
            if (maxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(currentHealth / maxHealth);
        }
    }

    public bool IsDead => isDead;

    private void Awake()
    {
        EnsureSupportComponents();
    }

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
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void PlayFireFeedback()
    {
        if (statusVfx != null)
        {
            statusVfx.EmitFireSparks();
        }
    }

    public void PlaySlowFeedback()
    {
        if (statusVfx != null)
        {
            statusVfx.EmitFrostShards();
        }
    }

    private void EnsureSupportComponents()
    {
        if (GetComponent<EnemyHealthBar>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar>();
        }

        statusVfx = GetComponent<EnemyStatusVfx>();
        if (statusVfx == null)
        {
            statusVfx = gameObject.AddComponent<EnemyStatusVfx>();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        WaveSpawner.ActiveEnemies.Remove(this);

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


