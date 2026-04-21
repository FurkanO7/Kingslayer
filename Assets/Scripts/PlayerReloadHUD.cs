using UnityEngine;
using TMPro;

public class PlayerReloadHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text reloadText;
    [SerializeField] private string reloadingText = "RELOADING...";
    [SerializeField] private bool showPercentage;

    private PlayerShooter playerShooter;
    private bool wasReloadingLastFrame;

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        playerShooter = FindFirstObjectByType<PlayerShooter>();
    }

    // Aktualisiert die Logik in jedem Frame.
    private void Update()
    {
        if (playerShooter == null)
        {
            return;
        }

        bool isReloading = playerShooter.IsReloading;
        float reloadProgress = playerShooter.ReloadProgress;

        if (reloadText != null)
        {
            if (isReloading)
            {
                if (showPercentage)
                {
                    int percentage = Mathf.RoundToInt(reloadProgress * 100f);
                    reloadText.text = reloadingText + " " + percentage + "%";
                }
                else
                {
                    reloadText.text = reloadingText;
                }
                reloadText.gameObject.SetActive(true);
            }
            else if (wasReloadingLastFrame)
            {
                reloadText.gameObject.SetActive(false);
            }
        }

        wasReloadingLastFrame = isReloading;
    }
}
