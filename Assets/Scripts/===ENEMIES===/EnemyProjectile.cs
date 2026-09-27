using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSFX;

    private Vector2 moveDirection = Vector2.left;
    private Rigidbody2D rb;

    public void Initialize(Vector2 direction, float projSpeed, float projDamage)
    {
        moveDirection = direction.normalized;
        speed = projSpeed;
        damage = projDamage;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = moveDirection * speed;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = moveDirection * speed;
            }
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb == null)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Impact();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.CompareTag("Ground"))
        {
            Impact();
        }
    }

    private void Impact()
    {
        if (hitSFX != null)
        {
            AudioManager am = Object.FindFirstObjectByType<AudioManager>();
            if (am != null) am.PlaySFX(hitSFX, transform.position);
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
