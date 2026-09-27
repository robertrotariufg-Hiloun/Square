using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private bool destroyOnHit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamage(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    private void DealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
