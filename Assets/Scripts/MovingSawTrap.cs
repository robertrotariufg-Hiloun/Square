using UnityEngine;

public class MovingSawTrap : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private bool instantKill = false;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Ruta de Movimiento")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float waitTimeAtPoint = 0.5f;

    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    private float waitTimer = 0f;

    void Update()
    {
        // Rotación continua
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Movimiento por waypoints
        if (waypoints == null || waypoints.Length < 2) return;

        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            waitTimer = waitTimeAtPoint;

            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = waypoints.Length - 2;
                    movingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    movingForward = true;
                }
            }
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

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
