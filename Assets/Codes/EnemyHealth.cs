using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public int moneyReward = 10;

    public EnemyMovement Movement { get; private set; }

    private bool isDead = false;
    private EnemyStatusVfx statusVfx;

    public float HealthNormalized => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

    public bool IsDead => isDead;

    private void Awake()
    {
        Movement = GetComponent<EnemyMovement>();
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
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
            Die();
    }

    public void PlayFireFeedback()
    {
        statusVfx?.EmitFireSparks();
    }

    public void PlaySlowFeedback()
    {
        statusVfx?.EmitFrostShards();
    }

    private void EnsureSupportComponents()
    {
        if (GetComponent<EnemyHealthBar>() == null)
            gameObject.AddComponent<EnemyHealthBar>();

        statusVfx = GetComponent<EnemyStatusVfx>();
        if (statusVfx == null)
            statusVfx = gameObject.AddComponent<EnemyStatusVfx>();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        WaveSpawner.ActiveEnemies.Remove(this);
        WaveSpawner.enemiesKilled++;

        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(moneyReward);

        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.EnemyRemoved();

        Destroy(gameObject);
    }
}
