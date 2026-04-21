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

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDeath;
        }
    }

    // Player dead -> pausiert das Spiel und zeigt das GameOver-Panel.
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

    // Lädt das aktuelle Level neu und hebt Pause auf.
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Lädt das Hauptmenü und hebt Pause auf.
    public void LoadMainMenu(string sceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
