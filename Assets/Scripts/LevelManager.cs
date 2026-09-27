using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private AudioManager audioManager;
    private PlayerController2D player;

    [Header("UI Partida")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image healthBar;

    [Header("Paneles")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winTimerText;
    [SerializeField] private TextMeshProUGUI winScoreText;

    [Header("Audio")]
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip gameOverMusic;

    public float levelTime;
    public int deathCount;
    public int score;
    private int bonusScore;
    private Vector3 currentRespawnPoint;
    private bool isLevelCompleted;

    void Awake()
    {
        instance = this;
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        player = Object.FindFirstObjectByType<PlayerController2D>();

        if (player != null)
        {
            currentRespawnPoint = player.transform.position;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        if (audioManager != null && levelMusic != null)
        {
            audioManager.PlayMusic(levelMusic);
        }
    }

    void Update()
    {
        if (isLevelCompleted) return;

        levelTime += Time.deltaTime;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        int minutes = Mathf.FloorToInt(levelTime / 60f);
        int seconds = Mathf.FloorToInt(levelTime % 60f);

        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (deathsText != null)
            deathsText.text = "Muertes: " + deathCount;

        if (scoreText != null)
            scoreText.text = "Puntos: " + CalculateScore();

        if (healthBar != null && player != null && player.maxHealth > 0)
            healthBar.fillAmount = player.health / player.maxHealth;
    }

    public int CalculateScore()
    {
        int baseScore = 10000;
        int timePenalty = Mathf.FloorToInt(levelTime * 10f);
        int deathPenalty = deathCount * 250;
        score = Mathf.Max(0, baseScore - timePenalty - deathPenalty + bonusScore);
        return score;
    }

    public void AddScore(int points)
    {
        bonusScore += points;
    }

    public void SetCheckpoint(Vector3 newPoint)
    {
        currentRespawnPoint = newPoint;
    }

    public Vector3 GetRespawnPoint()
    {
        return currentRespawnPoint;
    }

    public void PlayerDied()
    {
        deathCount++;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        if (audioManager != null)
        {
            audioManager.StopMusic();
            if (gameOverMusic != null)
                audioManager.PlayMusic(gameOverMusic, false);
        }
    }

    public void CompleteLevel()
    {
        isLevelCompleted = true;
        Time.timeScale = 0f;

        if (winPanel != null)
            winPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(levelTime / 60f);
        int seconds = Mathf.FloorToInt(levelTime % 60f);

        if (winTimerText != null)
            winTimerText.text = "Tiempo: " + string.Format("{0:00}:{1:00}", minutes, seconds);

        if (winScoreText != null)
            winScoreText.text = "Puntuacion: " + CalculateScore();

        if (audioManager != null)
        {
            audioManager.StopMusic();
            if (winMusic != null)
                audioManager.PlayMusic(winMusic, false);
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel(string nextSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}