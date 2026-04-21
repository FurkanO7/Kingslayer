using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPanelOpener : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool hidePanelsOnStart = true;
    [SerializeField] private bool forceVisibleCursorInMenu = true;

    // Initialisiert Referenzen und Startzustand beim Szenenstart.
    private void Start()
    {
        if (forceVisibleCursorInMenu)
        {
            ShowCursor();
        }

        if (!hidePanelsOnStart)
        {
            return;
        }

        SetPanelActive(levelSelectionPanel, false);
        SetPanelActive(settingsPanel, false);
    }

    // Enthaelt die Logik fuer OnPlayClicked.
    public void OnPlayClicked()
    {
        if (forceVisibleCursorInMenu)
        {
            ShowCursor();
        }

        SetPanelActive(levelSelectionPanel, true);
    }

    // Enthaelt die Logik fuer OnReturnFromLevelSelectionClicked.
    public void OnReturnFromLevelSelectionClicked()
    {
        SetPanelActive(levelSelectionPanel, false);
    }

    // Enthaelt die Logik fuer OnSettingsClicked.
    public void OnSettingsClicked()
    {
        if (forceVisibleCursorInMenu)
        {
            ShowCursor();
        }

        SetPanelActive(settingsPanel, true);
    }

    // Enthaelt die Logik fuer OnReturnFromSettingsClicked.
    public void OnReturnFromSettingsClicked()
    {
        SetPanelActive(settingsPanel, false);
    }

    // Enthaelt die Logik fuer OnExitClicked.
    public void OnExitClicked()
    {
        Application.Quit();
    }

    // Enthaelt die Logik fuer OnLoadTheRangeClicked.
    public void OnLoadTheRangeClicked()
    {
        SceneManager.LoadScene("TheRange");
    }

    // Setzt den Wert oder Zustand fuer PanelActive.
    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel == null)
        {
            return;
        }

        if (isActive)
        {
            ActivateParents(panel.transform);
        }

        panel.SetActive(isActive);
    }

    // Enthaelt die Logik fuer ActivateParents.
    private void ActivateParents(Transform child)
    {
        Transform current = child.parent;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
    }

    // Enthaelt die Logik fuer ShowCursor.
    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
