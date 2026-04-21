using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillLerpSpeed = 8f;

    private float targetFill = 1f;

    // Registriert Events und aktiviert benoetigte Eingaben.
    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += HandleHealthChanged;
            RefreshInstant();
        }
    }

    // Entfernt Event-Registrierungen und deaktiviert Eingaben.
    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= HandleHealthChanged;
        }
    }

    // Aktualisiert die Logik in jedem Frame.
    private void Update()
    {
        if (!smoothFill || healthFillImage == null)
        {
            return;
        }

        healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * fillLerpSpeed);
    }

    // Setzt den Wert oder Zustand fuer PlayerHealth.
    public void SetPlayerHealth(PlayerHealth newPlayerHealth)
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= HandleHealthChanged;
        }

        playerHealth = newPlayerHealth;

        if (playerHealth != null)
        {
            playerHealth.HealthChanged += HandleHealthChanged;
            RefreshInstant();
        }
    }

    // Aktualisiert Instant.
    private void RefreshInstant()
    {
        if (playerHealth == null)
        {
            return;
        }

        HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = targetFill;
        }
    }

    // Verarbeitet HealthChanged.
    private void HandleHealthChanged(int current, int max)
    {
        float normalized = max > 0 ? (float)current / max : 0f;
        targetFill = Mathf.Clamp01(normalized);

        if (!smoothFill && healthFillImage != null)
        {
            healthFillImage.fillAmount = targetFill;
        }

        if (healthText != null)
        {
            healthText.text = current + " / " + max;
        }
    }
}
