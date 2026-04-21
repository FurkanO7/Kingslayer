using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverPanel;

    private bool pauseGameOnDeath = true;

    private bool handledDeath;

    // Initialisiert Referenzen und versteckt das GameOver-Panel beim Start.
    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Registriert den Death-Event-Listener beim Aktivieren des Objekts.
    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDeath;
        }
    }

    // Entfernt den Death-Event-Listener beim Deaktivieren des Objekts.
    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDeath;
        }
    }

    // Reagiert auf den Tod des Spielers: pausiert das Spiel und zeigt das GameOver-Panel.
    private void HandlePlayerDeath()
    {
        if (handledDeath)
        {
            return;
        }

        handledDeath = true;

        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // LÃ¤dt das aktuelle Level neu und hebt vorher die Pause auf.
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // LÃ¤dt das HauptmenÃ¼ und hebt vorher die Pause auf.
    public void LoadMainMenu(string sceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
