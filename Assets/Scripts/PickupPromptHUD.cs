using UnityEngine;
using TMPro;

public class PickupPromptHUD : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private string promptFormat = "Aufnehmen";

    public static PickupPromptHUD Instance { get; private set; }

    // Initialisiert Referenzen und Startwerte.
    private void Awake()
    {
        Instance = this;
        Hide();
    }

    // Enthaelt die Logik fuer Show.
    public void Show(string weaponName)
    {
        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (weaponNameText != null)
            weaponNameText.text = weaponName + "\n" + promptFormat;
    }

    // Enthaelt die Logik fuer Hide.
    public void Hide()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}
