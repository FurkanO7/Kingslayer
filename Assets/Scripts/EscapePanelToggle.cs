using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapePanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] private bool hidePanelOnStart = true;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    private bool pauseGameWhileOpen = true;
    private bool showCursorWhileOpen = true;
    private bool lockCursorWhileClosed = true;

    // Initialisiert Referenzen und Startzustand beim Szenenstart.
    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (panel != null && hidePanelOnStart)
        {
            panel.SetActive(false);
        }

        ApplyCursorState(isPanelOpen: panel != null && panel.activeSelf);
    }

    // Registriert Events und aktiviert benoetigte Eingaben.
    private void OnEnable()
    {
        InputAction action = GetToggleAction();
        if (action == null)
        {
            return;
        }

        action.performed += OnTogglePerformed;
        action.Enable();
    }

    // Entfernt Event-Registrierungen und deaktiviert Eingaben.
    private void OnDisable()
    {
        InputAction action = GetToggleAction();
        if (action == null)
        {
            return;
        }

        action.performed -= OnTogglePerformed;
        action.Disable();
    }

    // Schaltet Panel zwischen den Zustaenden um.
    public void TogglePanel()
    {
        if (IsPlayerDead())
        {
            return;
        }

        if (panel == null)
        {
            return;
        }

        bool shouldOpen = !panel.activeSelf;
        panel.SetActive(shouldOpen);

        if (pauseGameWhileOpen)
        {
            Time.timeScale = shouldOpen ? 0f : 1f;
        }

        if (playerMovement != null)
        {
            playerMovement.SetLookEnabled(!shouldOpen);
        }

        ApplyCursorState(shouldOpen);
    }

    // Enthaelt die Logik fuer OnMenuButtonClicked.
    public void OnMenuButtonClicked()
    {
        TogglePanel();
    }

    // Enthaelt die Logik fuer OnLoadMainMenuClicked.
    public void OnLoadMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Oeffnet Panel.
    public void OpenPanel()
    {
        if (panel != null && !panel.activeSelf)
        {
            TogglePanel();
        }
    }

    // Schliesst Panel.
    public void ClosePanel()
    {
        if (panel != null && panel.activeSelf)
        {
            TogglePanel();
        }
    }

    // Enthaelt die Logik fuer OnTogglePerformed.
    private void OnTogglePerformed(InputAction.CallbackContext _)
    {
        if (IsPlayerDead())
        {
            return;
        }

        TogglePanel();
    }

    // Liefert ToggleAction zurueck.
    private InputAction GetToggleAction()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            return toggleAction.action;
        }

        return null;
    }

    // Wendet CursorState an.
    private void ApplyCursorState(bool isPanelOpen)
    {
        if (isPanelOpen)
        {
            if (showCursorWhileOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            return;
        }

        if (lockCursorWhileClosed)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Prueft den Zustand: PlayerDead.
    private bool IsPlayerDead()
    {
        return playerHealth != null && playerHealth.IsDead;
    }
}
