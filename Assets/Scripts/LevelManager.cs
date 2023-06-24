using static GameManager;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;

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
            StartCoroutine(StartLevelTransition(level));
        }
        else
        {
            Debug.LogError("Invalid level number: " + level);
        }
    }


    private IEnumerator StartLevelTransition(int level)
    {
        yield return ScreenManager.Instance.PlayStartTransition();
        bikeController.shouldMove = false; // Make sure the bike doesn't move during the transition
        LoadLevel(level);
        yield return new WaitForSeconds(0.5f);  // optional delay
        StartCoroutine(ScreenManager.Instance.PlayEndTransition());
    }


    private void LoadLevel(int level)
    {
        // Delete the previous level instance if it exists
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // Instantiate the new level
        currentLevelInstance = Instantiate(levels[currentLevel].levelPrefab);
        currentLevelInstance.SetActive(true);

        // Find the bike's starting position in the new level instance
        Transform bikeStartPosition = currentLevelInstance.transform.GetChild(0);
        GameManager.Instance.ResetLevelStats();
        // Set the bike's position to the starting position for the level
        bikeController.transform.position = bikeStartPosition.position;
        bikeController.transform.rotation = bikeStartPosition.rotation;

        BikeController.Instance.RB_Bike.velocity = Vector2.zero;
        BikeController.Instance.RB_Bike.angularVelocity = 0f;
        BikeController.Instance.accelerationTimer = 0f;
        BikeController.Instance.shouldMove = false;
        BikeController.Instance.PauseBike();

        ScreenManager.Instance.TweenMainMenu(false);
        GameManager.Instance.SetGameState(GameState.Starting);
        ScreenManager.Instance.TweenGameHUD(true);
        GameManager.Instance.ResetLevelStats();
    }

}

// Other functions to handle level progression, scoring, etc.
