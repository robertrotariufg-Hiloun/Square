using UnityEngine;

public class FlyingChaserEnemy : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    private AudioManager audioManager;
    private Transform playerTransform;

    [Header("Stats")]
    [SerializeField] private float health = 25f;
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float damage = 15f;

    [Header("Detección")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Efectos")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    private Vector3 initialPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;

        audioManager = Object.FindFirstObjectByType<AudioManager>();
        initialPosition = transform.position;
        health = maxHealth;

        PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
        if (player != null) playerTransform = player.transform;
    }

    void FixedUpdate()
    {
        if (playerTransform == null)
        {
            PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
            if (player != null) playerTransform = player.transform;
        }

        float distToPlayer = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : float.MaxValue;

        if (distToPlayer <= detectionRadius && playerTransform.gameObject.activeSelf)
        {
            // Perseguir al jugador
            Vector2 targetDir = (playerTransform.position - transform.position).normalized;
            if (rb != null)
            {
                rb.linearVelocity = targetDir * chaseSpeed;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.fixedDeltaTime);
            }

            // Girar sprite mirando al jugador
            if (targetDir.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (targetDir.x > 0 ? 1 : -1);
                transform.localScale = scale;
            }
        }
        else
        {
            // Regresar al punto inicial
            float distToHome = Vector2.Distance(transform.position, initialPosition);
            if (distToHome > 0.1f)
            {
                Vector2 targetDir = (initialPosition - transform.position).normalized;
                if (rb != null)
                {
                    rb.linearVelocity = targetDir * returnSpeed;
                }
                else
                {
                    transform.position = Vector2.MoveTowards(transform.position, initialPosition, returnSpeed * Time.fixedDeltaTime);
                }
            }
            else
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPlayerHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlayerHit(collision.gameObject);
    }

    private void CheckPlayerHit(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            PlayerController2D player = target.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
