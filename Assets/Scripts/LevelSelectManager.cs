using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    private const string Level2Key = "Level2Unlocked";
    private const string Level3Key = "Level3Unlocked";

    [Header("Level Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("Scene Names")]
    [SerializeField] private string level1SceneName = "Level_1";
    [SerializeField] private string level2SceneName = "Level_2";
    [SerializeField] private string level3SceneName = "Level_3";

    private void OnEnable()
    {
        RefreshButtonStates();
    }

    // Aktualisiert ButtonStates.
    private void RefreshButtonStates()
    {
        bool level2Unlocked = PlayerPrefs.GetInt(Level2Key, 0) == 1;
        bool level3Unlocked = PlayerPrefs.GetInt(Level3Key, 0) == 1;

        if (level2Button != null)
            level2Button.interactable = level2Unlocked;

        if (level3Button != null)
            level3Button.interactable = level3Unlocked;
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    public void LoadLevel2()
    {
        if (PlayerPrefs.GetInt(Level2Key, 0) == 1)
            SceneManager.LoadScene(level2SceneName);
    }

    public void LoadLevel3()
    {
        if (PlayerPrefs.GetInt(Level3Key, 0) == 1)
            SceneManager.LoadScene(level3SceneName);
    }


    public static void UnlockLevel2()
    {
        PlayerPrefs.SetInt(Level2Key, 1);
        PlayerPrefs.Save();
    }

    public static void UnlockLevel3()
    {
        PlayerPrefs.SetInt(Level3Key, 1);
        PlayerPrefs.Save();
    }

    // Setzt alle Level-Freischaltungen zurück und aktualisiert die Buttons.
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(Level2Key);
        PlayerPrefs.DeleteKey(Level3Key);
        PlayerPrefs.Save();
        RefreshButtonStates();
    }
}
