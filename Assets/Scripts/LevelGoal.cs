using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string lobbySceneName = "MainMenu";

    [Header("Level Unlock")]
    [SerializeField] private bool unlockNextLevel = true;
    [SerializeField] private int thisLevelNumber = 1; // 1 unlocks Level 2, 2 unlocks Level 3

    // Reagiert auf Trigger-Eintritte.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (unlockNextLevel)
        {
            if (thisLevelNumber == 1)
            {
                LevelSelectManager.UnlockLevel2();
            }
            else if (thisLevelNumber == 2)
            {
                LevelSelectManager.UnlockLevel3();
            }
        }

        SceneManager.LoadScene(lobbySceneName);
    }
}
