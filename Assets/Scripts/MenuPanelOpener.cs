using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPanelOpener : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool hidePanelsOnStart = true;
    [SerializeField] private bool forceVisibleCursorInMenu = true;

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

    public void OnPlayClicked()
    {
        if (forceVisibleCursorInMenu)
        {
            ShowCursor();
        }

        SetPanelActive(levelSelectionPanel, true);
    }

    public void OnReturnFromLevelSelectionClicked()
    {
        SetPanelActive(levelSelectionPanel, false);
    }

    public void OnSettingsClicked()
    {
        if (forceVisibleCursorInMenu)
        {
            ShowCursor();
        }

        SetPanelActive(settingsPanel, true);
    }

    public void OnReturnFromSettingsClicked()
    {
        SetPanelActive(settingsPanel, false);
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnLoadTheRangeClicked()
    {
        SceneManager.LoadScene("TheRange");
    }

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

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
