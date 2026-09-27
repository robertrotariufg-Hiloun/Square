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

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    private float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation + rotationSpeed * Time.fixedDeltaTime);

        if (waypoints.Length < 2) return;

        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector2 newPos = Vector2.MoveTowards(rb.position, target.position, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, target.position) < 0.05f)
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
            if (instantKill)
                player.Die();
            else
                player.TakeDamage(damage);
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}