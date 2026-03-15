using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Base")]
    public int baseHP = 10;

    [Header("Money")]
    public int money = 200;

    [Header("References")]
    public WaveSpawner waveSpawner;

    [Header("UI")]
    public TMP_Text baseHpText;
    public TMP_Text killText;
    public TMP_Text levelText;
    public TMP_Text moneyText;
    public TMP_Text waveText;
    public TMP_Text nextWaveText;
    public GameObject gameOverPanel;

    private bool gameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    public void DamageBase(int damage)
    {
        if (gameOver) return;

        baseHP -= damage;

        if (baseHP <= 0)
        {
            baseHP = 0;
            GameOver();
        }

        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount)
            return false;

        money -= amount;
        UpdateUI();
        return true;
    }

    void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (baseHpText != null)
            baseHpText.text = "Base HP: " + baseHP;

        if (killText != null)
            killText.text = "Kills: " + WaveSpawner.enemiesKilled;

        if (levelText != null)
            levelText.text = "Level: " + SceneManager.GetActiveScene().name;

        if (moneyText != null)
            moneyText.text = "Money: " + money;

        if (waveSpawner != null && waveText != null)
            waveText.text = "Wave: " + waveSpawner.currentWave;

        if (waveSpawner != null && nextWaveText != null)
        {
            if (waveSpawner.nextWaveCountdown > 0.1f)
                nextWaveText.text = "Next Wave In: " + Mathf.CeilToInt(waveSpawner.nextWaveCountdown);
            else
                nextWaveText.text = "";
        }
    }
}
