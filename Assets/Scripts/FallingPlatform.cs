using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float shakeIntensity = 0.05f;

    private Vector3 initialPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private bool isTriggered = false;

    void Awake()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Verificar si el jugador pisó la plataforma por arriba
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(FallRoutine());
                    break;
                }
            }
        }
    }

    private IEnumerator FallRoutine()
    {
        isTriggered = true;

        // Fase de temblor
        float elapsed = 0f;
        while (elapsed < fallDelay)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = initialPosition + new Vector3(offsetX, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition;

        // Caída
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            // Si no tiene Rigidbody2D, ocultamos y desactivamos collider
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        // Restaurar estado original
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;

        if (col != null) col.enabled = true;
        if (sr != null) sr.enabled = true;

        isTriggered = false;
    }
}
