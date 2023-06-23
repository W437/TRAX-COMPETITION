using static GameManager;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance

    public Level[] levels; // Assign in Unity Editor

    public int currentLevel = 0;
    private GameObject currentLevelInstance;
    private BikeController bikeController;

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

        // You may want to get a reference to the BikeController here
        bikeController = FindObjectOfType<BikeController>();
    }

    public Level GetCurrentLevelData()
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
        // Check if the level number is valid
        if (level >= 0 && level < levels.Length)
        {
            currentLevel = level;

            // Delete the previous level instance if it exists
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
            }

            // Instantiate the new level
            currentLevelInstance = Instantiate(levels[currentLevel].levelPrefab);

            // Find the bike's starting position in the new level instance
            Transform bikeStartPosition = currentLevelInstance.transform.GetChild(0);

            // Set the bike's position to the starting position for the level
            bikeController.transform.position = bikeStartPosition.position;

            ScreenManager.Instance.TweenMainMenu(false);
            GameManager.Instance.SetGameState(GameState.Playing);
            ScreenManager.Instance.TweenGameHUD(true);
            GameManager.Instance.ResetLevelStats();
        }
        else
        {
            Debug.LogError("Invalid level number: " + level);
        }
    }

    // Other functions to handle level progression, scoring, etc.
}
