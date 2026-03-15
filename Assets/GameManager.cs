using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Base")]
    public int baseHP = 10;

    [Header("UI")]
    public TMP_Text baseHpText;
    public TMP_Text killText;
    public TMP_Text levelText;
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
    }
}