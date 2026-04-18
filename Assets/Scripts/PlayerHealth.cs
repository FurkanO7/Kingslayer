using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 250;
    [SerializeField] private int damagePerEnemyBullet = 20;

    private int currentHealth;

    public event Action<int, int> HealthChanged;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
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
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Debug.Log("Player ist gestorben.");
            return;
        }

        Debug.Log($"Player wurde getroffen! Leben: {currentHealth}/{maxHealth}");
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        NotifyHealthChanged();

        Debug.Log($"Player wurde geheilt! Leben: {currentHealth}/{maxHealth}");
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
