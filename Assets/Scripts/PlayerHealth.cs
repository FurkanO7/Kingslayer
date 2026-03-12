using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 150;
    [SerializeField] private int damagePerEnemyBullet = 20;

    private int currentHealth;

    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTakeBulletDamage(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTakeBulletDamage(other);
    }

    private void TryTakeBulletDamage(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return;
        }

        Projectile projectile = hitCollider.GetComponent<Projectile>();
        if (projectile == null)
        {
            return;
        }

        if (projectile.OwnerTag != "Enemy")
        {
            return;
        }

        TakeDamage(damagePerEnemyBullet);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player ist gestorben.");
            return;
        }

        Debug.Log($"Player wurde getroffen! Leben: {currentHealth}/{maxHealth}");
    }
}
