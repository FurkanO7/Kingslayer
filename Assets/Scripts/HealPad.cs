using UnityEngine;

public class HealPad : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 50;
    [SerializeField] private bool consumeOnUse = true;

    // Reagiert auf Trigger-Eintritte.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.Heal(healAmount);

        if (consumeOnUse)
        {
            Destroy(gameObject);
        }
    }
}
