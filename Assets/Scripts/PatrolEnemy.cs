using UnityEngine;

public class PatrolEnemy : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    private AudioManager audioManager;

    [Header("Stats")]
    [SerializeField] private float health = 30f;
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damage = 20f;

    [Header("Detección de Bordes y Muros")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Efectos")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    private int direction = 1; // 1 = Derecha, -1 = Izquierda

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        health = maxHealth;

        if (groundLayer.value == 0)
        {
            int defaultLayer = LayerMask.NameToLayer("Default");
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            int mask = 0;
            if (defaultLayer != -1) mask |= (1 << defaultLayer);
            if (groundLayerIndex != -1) mask |= (1 << groundLayerIndex);
            if (mask != 0) groundLayer = mask;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        Vector2 originGround = groundCheck != null ? groundCheck.position : transform.position + new Vector3(direction * 0.4f, 0, 0);
        Vector2 originWall = wallCheck != null ? wallCheck.position : transform.position;

        RaycastHit2D groundHit = Physics2D.Raycast(originGround, Vector2.down, checkDistance, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(originWall, Vector2.right * direction, checkDistance, groundLayer);

        // Si no hay suelo adelante o choca con muro -> gira
        if (!groundHit.collider || wallHit.collider)
        {
            Flip();
        }
    }

    private void Flip()
    {
        direction *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
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

    private void OnDrawGizmosSelected()
    {
        Vector2 originGround = groundCheck != null ? groundCheck.position : transform.position + new Vector3(direction * 0.4f, 0, 0);
        Vector2 originWall = wallCheck != null ? wallCheck.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(originGround, originGround + Vector2.down * checkDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(originWall, originWall + Vector2.right * direction * checkDistance);
    }
}
