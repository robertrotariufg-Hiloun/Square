using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private AudioClip checkpointSFX;
    [SerializeField] private GameObject activateEffect;

    private SpriteRenderer sr;
    private bool isActivated = false;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated) return;

        if (collision.CompareTag("Player"))
        {
            isActivated = true;

            if (LevelManager.instance != null)
            {
                LevelManager.instance.SetCheckpoint(transform.position);
            }

            if (activeSprite != null && sr != null)
            {
                sr.sprite = activeSprite;
            }

            if (activateEffect != null)
            {
                Instantiate(activateEffect, transform.position, Quaternion.identity);
            }

            if (checkpointSFX != null)
            {
                AudioManager am = Object.FindFirstObjectByType<AudioManager>();
                if (am != null) am.PlaySFX(checkpointSFX, transform.position);
            }
        }
    }
}
