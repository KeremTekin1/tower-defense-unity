using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        EnsureRuntimeSupportObject<WavePanelUI>("WavePanelUI");
        EnsureRuntimeSupportObject<TowerSelectionUI>("TowerSelectionUI");
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void DamageBase(int damage)
    {
        if (gameOver)
        {
            return;
        }

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
        {
            return false;
        }

        money -= amount;
        UpdateUI();
        return true;
    }

    private void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        WaveSpawner.ActiveEnemies.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateUI()
    {
        if (baseHpText != null)
        {
            baseHpText.text = BuildLabel("BASE", "#7DD3FC", baseHP.ToString());
        }

        if (killText != null)
        {
            killText.text = BuildLabel("KILLS", "#FCA5A5", WaveSpawner.enemiesKilled.ToString());
        }

        if (levelText != null)
        {
            levelText.text = BuildLabel("LEVEL", "#F8FAFC", GetDisplayLevelValue());
        }

        if (moneyText != null)
        {
            moneyText.text = BuildLabel("CASH", "#FDE68A", money.ToString());
        }

        if (waveSpawner != null && waveText != null)
        {
            waveText.text = BuildLabel("WAVE", "#C4B5FD", waveSpawner.currentWave.ToString());
        }

        if (waveSpawner != null && nextWaveText != null)
        {
            if (waveSpawner.nextWaveCountdown > 0.1f)
            {
                nextWaveText.text = BuildLabel("NEXT", "#86EFAC", Mathf.CeilToInt(waveSpawner.nextWaveCountdown) + "s");
            }
            else
            {
                nextWaveText.text = string.Empty;
            }
        }
    }

    private string GetDisplayLevelValue()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string numericPart = string.Empty;

        for (int i = 0; i < sceneName.Length; i++)
        {
            if (char.IsDigit(sceneName[i]))
            {
                numericPart += sceneName[i];
            }
        }

        return string.IsNullOrEmpty(numericPart) ? sceneName.ToUpperInvariant() : numericPart;
    }

    private string BuildLabel(string title, string colorHex, string value)
    {
        return $"<size=82%><b><color={colorHex}>{title}</color></b></size>  <size=102%>{value}</size>";
    }

    private void EnsureRuntimeSupportObject<T>(string objectName) where T : Component
    {
        if (FindFirstObjectByType<T>() != null)
        {
            return;
        }

        GameObject supportObject = new GameObject(objectName);
        supportObject.AddComponent<T>();
    }
}
