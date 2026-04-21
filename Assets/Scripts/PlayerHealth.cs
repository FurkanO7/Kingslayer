using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 250;
    [SerializeField] private int damagePerEnemyBullet = 20;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;

    private int currentHealth;
    private bool isDead;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
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

    // Prüft Bedingungen und führt TakeBulletDamage bei Erfolg aus.
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

    // Verarbeitet den erlittenen Schaden, spielt Sound ab und löst Events aus.
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            isDead = true;
            Died?.Invoke();
            return;
        }

    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0 || currentHealth >= maxHealth)
        {
            return false;
        }

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        NotifyHealthChanged();

        return true;

    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
