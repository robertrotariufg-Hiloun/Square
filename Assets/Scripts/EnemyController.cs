using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    private AudioManager audioManager;

    [Header("Stats")]
    [SerializeField] private float health = 50f;
    [SerializeField] private float damage = 20f;

    [Header("Efectos")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    void Awake()
    {
        audioManager = Object.FindFirstObjectByType<AudioManager>();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (damageSFX != null && audioManager != null)
            audioManager.PlaySFX(damageSFX, transform.position, Random.Range(0.9f, 1.1f));

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (deathSFX != null && audioManager != null)
            audioManager.PlaySFX(deathSFX, transform.position);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}
