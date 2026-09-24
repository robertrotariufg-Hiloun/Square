using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private float damage = 100f; // Por defecto daño mortal
    [SerializeField] private bool instantKill = true;

    [Header("Pinchos Temporizados (Opcional)")]
    [SerializeField] private bool isTimed = false;
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float inactiveTime = 2f;
    [SerializeField] private SpriteRenderer spikeRenderer;
    [SerializeField] private Collider2D spikeCollider;

    private float timer;
    private bool isActive = true;

    void Awake()
    {
        if (spikeCollider == null)
            spikeCollider = GetComponent<Collider2D>();

        if (spikeRenderer == null)
            spikeRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (!isTimed) return;

        timer += Time.deltaTime;
        if (isActive && timer >= activeTime)
        {
            SetState(false);
            timer = 0f;
        }
        else if (!isActive && timer >= inactiveTime)
        {
            SetState(true);
            timer = 0f;
        }
    }

    private void SetState(bool active)
    {
        isActive = active;
        if (spikeCollider != null) spikeCollider.enabled = active;
        if (spikeRenderer != null)
        {
            Color c = spikeRenderer.color;
            c.a = active ? 1f : 0.2f;
            spikeRenderer.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamagePlayer(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamagePlayer(collision.gameObject);
    }

    private void DamagePlayer(GameObject target)
    {
        if (!isActive) return;

        if (target.CompareTag("Player"))
        {
            PlayerController2D player = target.GetComponent<PlayerController2D>();
            if (player != null)
            {
                if (instantKill)
                    player.Die();
                else
                    player.TakeDamage(damage);
            }
        }
    }
}
