using System.Collections;
using UnityEngine;

public class WindParticles : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform windParticles;
    [SerializeField] private float rotationSpeed = 5f; // Controla la velocidad del suavizado

    private int destinationIndex = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || windParticles == null) return;

        // 1. Calculamos la dirección hacia el waypoint actual
        Vector3 direction = waypoints[destinationIndex].position - windParticles.position;

        // Evitamos errores si el objeto está exactamente en la misma posición que el waypoint
        if (direction.sqrMagnitude > 0.001f)
        {
            // 2. Calculamos la rotación objetivo
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 3. Interpolamos de forma fluida la rotación actual hacia la objetivo
            windParticles.rotation = Quaternion.Slerp(windParticles.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si toca el waypoint actual, avanzamos al siguiente de forma inmediata en el índice
        if (other.CompareTag("Waypoint"))
        {
            if (destinationIndex < waypoints.Length - 1)
            {
                destinationIndex++; // El Update se encargará de suavizar la rotación hacia este nuevo índice
            }
        }

    }
}