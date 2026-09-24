using UnityEngine;

public enum CollectibleType
{
    Score,
    Health
}

public class CollectibleItem : MonoBehaviour
{
    [Header("Tipo y Valor")]
    [SerializeField] private CollectibleType type = CollectibleType.Score;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float healValue = 25f;

    [Header("Efecto de Flotación")]
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatFrequency = 2f;

    [Header("Efectos")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSFX;

    private Vector3 startPos;
    private AudioManager audioManager;

    void Awake()
    {
        startPos = transform.position;
        audioManager = Object.FindFirstObjectByType<AudioManager>();
    }

    void Update()
    {
        // Movimiento de flotación suave tipo seno
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                if (type == CollectibleType.Score)
                {
                    if (LevelManager.instance != null)
                        LevelManager.instance.AddScore(scoreValue);
                }
                else if (type == CollectibleType.Health)
                {
                    player.Heal(healValue);
                }

                if (pickupSFX != null && audioManager != null)
                    audioManager.PlaySFX(pickupSFX, transform.position, Random.Range(1f, 1.2f));

                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }
    }
}
