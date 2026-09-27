using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private GameObject winEffect;

    private bool isReached = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReached) return;

        if (collision.CompareTag("Player"))
        {
            isReached = true;

            if (winEffect != null)
                Instantiate(winEffect, transform.position, Quaternion.identity);

            if (winSFX != null)
            {
                AudioManager am = Object.FindFirstObjectByType<AudioManager>();
                if (am != null) am.PlaySFX(winSFX, transform.position);
            }

            if (LevelManager.instance != null)
            {
                LevelManager.instance.CompleteLevel();
            }
        }
    }
}
