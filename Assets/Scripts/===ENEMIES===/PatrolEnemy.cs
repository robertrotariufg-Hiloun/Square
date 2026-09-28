using System.Collections;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    private AudioManager audioManager;
    private Animator animator;
    private Transform playerTransform;

    [Header("Stats")]
    [SerializeField] private float health = 30f;
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float damage = 20f;

    [Header("Patrulla")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float waypointThreshold = 0.1f;

    [Header("Detección")]
    [SerializeField] private float detectionRadius = 5f;

    [Header("Ataque")]
    [SerializeField] private float attackStunDuration = 0.4f;

    [Header("Efectos")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    private Transform currentTarget;
    private int direction = 1;
    private bool isAttacking = false;
    private bool isChasing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        playerTransform = GameObject.Find("Player").transform;
        health = maxHealth;
        currentTarget = pointB;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -direction;
        transform.localScale = scale;
    }

    void FixedUpdate()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        if (isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isChasing = distToPlayer <= detectionRadius;

        if (isChasing)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    private void Chase()
    {
        float dirToPlayer = playerTransform.position.x - transform.position.x;
        int newDirection = dirToPlayer > 0 ? 1 : -1;

        if (newDirection != direction)
        {
            direction = newDirection;
            Flip();
        }

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    private void Patrol()
    {
        if (Mathf.Abs(transform.position.x - currentTarget.position.x) < waypointThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }

        int newDirection = currentTarget.position.x > transform.position.x ? 1 : -1;

        if (newDirection != direction)
        {
            direction = newDirection;
            Flip();
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -direction;
        transform.localScale = scale;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        audioManager.PlaySFX(damageSFX, transform.position, Random.Range(0.9f, 1.1f));

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        audioManager.PlaySFX(deathSFX, transform.position);
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            player.TakeDamage(damage);
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackStunDuration);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.15f);
        Gizmos.DrawWireSphere(pointB.position, 0.15f);
    }
}