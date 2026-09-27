using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float respawnDelay = 3f;

    [Header("Caída")]
    [SerializeField] private float fallDistance = 1.5f;
    [SerializeField] private float fallDuration = 0.4f;

    private Vector3 initialPosition;
    private Collider2D col;
    private SpriteRenderer sr;
    private bool isTriggered = false;

    void Awake()
    {
        initialPosition = transform.position;
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
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

        float elapsed = 0f;
        while (elapsed < fallDelay)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = initialPosition + new Vector3(offsetX, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * fallDistance;

        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / fallDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        col.enabled = false;
        sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = initialPosition;
        col.enabled = true;
        sr.enabled = true;

        isTriggered = false;
    }
}