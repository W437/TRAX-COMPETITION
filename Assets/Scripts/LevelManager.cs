using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance

    public LevelData[] levels; // Assign in Unity Editor

    private int currentLevel = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LevelData GetCurrentLevelData()
    {
        if (currentLevel < levels.Length)
        {
            return levels[currentLevel];
        }
        else
        {
            // Handle game completion or looping back to first level
            return null;
        }
    }

    public void StartLevel(int level)
    {
        ScreenManager.Instance.Panel_MainMenu.SetActive(false);
        ScreenManager.Instance.Panel_GameHUD.SetActive(true);
        GameManager.Instance.SetGameState(GameState.Playing);
    }
    // Other functions to handle level progression, scoring, etc.
}
