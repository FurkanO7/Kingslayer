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

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        NotifyHealthChanged();
    }

    // Reagiert auf Kollisionen mit anderen Objekten.
    private void OnCollisionEnter(Collision collision)
    {
        TryTakeBulletDamage(collision.collider);
    }

    // Reagiert auf Trigger-Eintritte.
    private void OnTriggerEnter(Collider other)
    {
        TryTakeBulletDamage(other);
    }

    // Prueft Bedingungen und fuehrt TakeBulletDamage nur bei Erfolg aus.
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

    // Enthaelt die Logik fuer TakeDamage.
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

    // Enthaelt die Logik fuer Heal.
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

    }

    // Informiert andere Systeme ueber HealthChanged.
    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
