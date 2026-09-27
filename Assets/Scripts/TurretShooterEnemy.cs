using UnityEngine;

public class TurretShooterEnemy : MonoBehaviour, IDamageable
{
    private AudioManager audioManager;
    private Transform playerTransform;

    [Header("Stats")]
    [SerializeField] private float health = 40f;
    [SerializeField] private float maxHealth = 40f;

    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private bool aimAtPlayer = true;

    [Header("Efectos")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    private float fireTimer;

    void Awake()
    {
        audioManager = Object.FindFirstObjectByType<AudioManager>();
        health = maxHealth;

        if (firePoint == null)
            firePoint = transform;

        PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
            if (player != null) playerTransform = player.transform;
        }

        fireTimer += Time.deltaTime;

        float distToPlayer = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : float.MaxValue;

        if (distToPlayer <= detectionRadius && playerTransform.gameObject.activeSelf)
        {
            // Girar sprite mirando al jugador
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            if (dirToPlayer.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dirToPlayer.x > 0 ? 1 : -1);
                transform.localScale = scale;
            }

            if (fireTimer >= fireRate)
            {
                Shoot(dirToPlayer);
                fireTimer = 0f;
            }
        }
    }

    private void Shoot(Vector2 dirToPlayer)
    {
        if (projectilePrefab == null) return;

        Vector2 shootDir = aimAtPlayer ? dirToPlayer : (transform.localScale.x > 0 ? Vector2.right : Vector2.left);
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Initialize(shootDir, projectileSpeed, projectileDamage);
        }

        if (shootSFX != null && audioManager != null)
        {
            audioManager.PlaySFX(shootSFX, transform.position, Random.Range(0.95f, 1.05f));
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
