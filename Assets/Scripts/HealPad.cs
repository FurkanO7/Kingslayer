using UnityEngine;

public class HealPad : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 50;
    [SerializeField] private bool consumeOnUse = true;

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

        bool wasHealed = playerHealth.Heal(healAmount);

        if (!wasHealed)
        {
            return;
        }

        if (consumeOnUse)
        {
            Destroy(gameObject);
        }
    }
}
