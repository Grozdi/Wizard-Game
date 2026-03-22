using UnityEngine;

/*
    ProjectileDamage.cs

    Added at runtime or attach manually to projectile prefabs.
    When the projectile hits an EnemySkeleton, it applies damage and destroys itself.
*/
public class ProjectileDamage : MonoBehaviour
{
    public int damage = 25;

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApplyDamage(other.gameObject);
    }

    private void TryApplyDamage(GameObject other)
    {
        EnemySkeleton enemy = other.GetComponentInParent<EnemySkeleton>();
        if (enemy == null)
        {
            return;
        }

        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
