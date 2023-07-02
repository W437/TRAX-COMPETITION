using static GameManager;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance
    public Transform levelsGameObject;
    public BackgroundParalax bgParalax;

    public TMPro.TextMeshProUGUI levelInfoText;



    [SerializeField] public Level[] levels;

    public int CurrentLevelID { get; private set; } = 0;
    public GameObject currentLevelInstance;

    void Awake()
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
        if (CurrentLevelID < levels.Length)
        {
            return levels[CurrentLevelID];
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
        CurrentLevelID = level;
        var currentLevelData = GetCurrentLevelData();
        var currentLevelCategory = currentLevelData.category;
        StartCoroutine(StartLevelTransition(level));
        CameraController.Instance.SwitchToGameCamera();

        // Stop the previous BikeAnimationInCoroutine if it exists
        if (ScreenManager.Instance.BikeAnimationInCoroutine != null)
        {
            ScreenManager.Instance.StopCoroutine(ScreenManager.Instance.BikeAnimationInCoroutine);
            ScreenManager.Instance.BikeAnimationInCoroutine = null;
        }

        // Stop the previous BikeAnimationOutCoroutine if it exists
        if (ScreenManager.Instance.BikeAnimationOutCoroutine != null)
        {
            ScreenManager.Instance.StopCoroutine(ScreenManager.Instance.BikeAnimationOutCoroutine);
            ScreenManager.Instance.BikeAnimationOutCoroutine = null;
        }
    }
    else
    {
        Debug.LogError("Invalid level number: " + level);
    }
}



    IEnumerator StartLevelTransition(int level)
    {
        yield return ScreenManager.Instance.PlayStartTransition();
        LoadLevel(level);

        // Enable the levelInfoText during the transition
        levelInfoText.gameObject.SetActive(true);

        // Update the levelInfoText with the level category and level number
        levelInfoText.text = levels[level].category.ToString() + " - Level " + (levels[level].levelID+1);

        yield return new WaitForSeconds(0.5f);  // optional delay

        // Disable the levelInfoText after the transition ends
        levelInfoText.gameObject.SetActive(false);
        StartCoroutine(ScreenManager.Instance.PlayEndTransition());
    }


    void LoadLevel(int level)
    {
        // Delete the previous level instance if it exists
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // Instantiate the new level
        currentLevelInstance = Instantiate(levels[CurrentLevelID].levelPrefab, levelsGameObject);
        currentLevelInstance.SetActive(true);

        // Find the finish line in the new level instance
        Transform finishLine = currentLevelInstance.transform.Find("Finish"); 
                                                                                
        bgParalax.SetFinishLine(finishLine);

        // Fetch PlayerData to get the selected bike ID
        PlayerData playerData = SaveSystem.LoadPlayerData();

        // Instantiate the bike
        BikeController.Instance.LoadPlayerBike(playerData.SELECTED_BIKE_ID);

        // Find the bike's starting position in the new level instance
        Transform bikeStartPosition = currentLevelInstance.transform.GetChild(0);
        GameManager.Instance.ResetLevelStats();
        // Set the bike's position to the starting position for the level

        GameManager.Instance.GAME_PlayerBike.SetActive(true);
        GameManager.Instance.GAME_PlayerBike.transform.SetPositionAndRotation(bikeStartPosition.position, bikeStartPosition.rotation);

        BikeController.Instance.PauseBike();

        ScreenManager.Instance.TweenMainMenu(false);
        GameManager.Instance.SetGameState(GameState.Starting);
        ScreenManager.Instance.TweenGameHUD(true);
        GameManager.Instance.ResetLevelStats();
        CameraController.Instance.SwitchToGameCamera();
    }

}

// Other functions to handle level progression, scoring, etc.
