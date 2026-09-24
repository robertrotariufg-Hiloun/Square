using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "SampleScene";
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip menuMusic;

    private AudioManager audioManager;

    void Start()
    {
        Time.timeScale = 1f;
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        if (audioManager != null && menuMusic != null)
        {
            audioManager.PlayMusic(menuMusic);
        }
    }

    public void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (buttonClickSFX != null && audioManager != null)
        {
            audioManager.PlayUISFX(buttonClickSFX);
        }
    }
}
