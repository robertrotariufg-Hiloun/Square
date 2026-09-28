using System.Collections;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfx;
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private float teleportDelay = 1f;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        Rigidbody playerRb = other.GetComponent<Rigidbody>();

        vfx.Play();
        StartCoroutine(WaitAndTeleport(playerRb));
    }

    IEnumerator WaitAndTeleport(Rigidbody playerRb)
    {
        yield return new WaitForSeconds(teleportDelay);

        playerRb.position = teleportTarget.position;
        playerRb.linearVelocity = Vector3.zero;
    }
}