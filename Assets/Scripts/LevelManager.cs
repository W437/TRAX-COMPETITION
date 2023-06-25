using static GameManager;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance
    public Transform levelsGameObject;

    public Level[] levels; // Assign in Unity Editor

    public int currentLevel = 0;
    private GameObject currentLevelInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("More than one instance of GameManager found!");
            Destroy(this.gameObject);
        }
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
            CameraController.Instance.SwitchToGameCamera();
        }
        else
        {
            Debug.LogError("Invalid level number: " + level);
        }
    }


    private IEnumerator StartLevelTransition(int level)
    {
        yield return ScreenManager.Instance.PlayStartTransition();
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
        currentLevelInstance = Instantiate(levels[currentLevel].levelPrefab, levelsGameObject);
        currentLevelInstance.SetActive(true);

        // Fetch PlayerData to get the selected bike ID
        PlayerData playerData = GameManager.Instance.GetPlayerData();

        // Instantiate the bike
        GameManager.Instance.LoadPlayerBike(playerData.selectedBikeId);


        // Find the bike's starting position in the new level instance
        Transform bikeStartPosition = currentLevelInstance.transform.GetChild(0);
        GameManager.Instance.ResetLevelStats();
        // Set the bike's position to the starting position for the level

        BikeController.Instance.PlayerBike.SetActive(true);
        BikeController.Instance.transform.position = bikeStartPosition.position;
        BikeController.Instance.transform.rotation = bikeStartPosition.rotation;

        BikeController.Instance.PauseBike();

        ScreenManager.Instance.TweenMainMenu(false);
        GameManager.Instance.SetGameState(GameState.Starting);
        ScreenManager.Instance.TweenGameHUD(true);
        GameManager.Instance.ResetLevelStats();
        CameraController.Instance.SwitchToGameCamera();
    }

}

// Other functions to handle level progression, scoring, etc.
