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
    public static int deathCount;
    public int score;
    private int bonusScore;
    private bool isLevelCompleted;

    void Awake()
    {
        instance = this;
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController2D>();
    }

    void Start()
    {
        Time.timeScale = 1f;
        audioManager.PlayMusic(levelMusic);
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

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        deathsText.text =  deathCount.ToString();
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

    public void PlayerDied()
    {
        deathCount++;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        audioManager.StopMusic();
        audioManager.PlayMusic(gameOverMusic, false);
    }

    public void CompleteLevel()
    {
        isLevelCompleted = true;
        Time.timeScale = 0f;

        winPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(levelTime / 60f);
        int seconds = Mathf.FloorToInt(levelTime % 60f);

        winTimerText.text = "Tiempo: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        winScoreText.text = "Puntuacion: " + CalculateScore();

        audioManager.StopMusic();
        audioManager.PlayMusic(winMusic, false);
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
        deathCount = 0;
        SceneManager.LoadScene("MainMenu");
    }
}