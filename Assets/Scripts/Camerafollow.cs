using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;

    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = player.position + offset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}