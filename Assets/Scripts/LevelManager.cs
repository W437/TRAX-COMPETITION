using static GameManager;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance
    public Transform LevelGameObject;
    public BackgroundParalax BackgroundParalax;
    public TMPro.TextMeshProUGUI Txt_LevelInfo;
    [SerializeField] public Level[] Levels;
    public int CurrentLevelID { get; private set; } = 0;
    public GameObject CurrentLevelInstance;


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
        if (CurrentLevelID < Levels.Length)
        {
            return Levels[CurrentLevelID];
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
        if (level >= 0 && level < Levels.Length)
        {
            CurrentLevelID = level;
            var currentLevelData = GetCurrentLevelData();
            var currentLevelCategory = currentLevelData.LevelCategory;
            StartCoroutine(StartLevelTransition(level));
            BackgroundParalax.ResetParallax();
            CameraController.Instance.SwitchToGameCamera();
            BackgroundParalax.ResetParallax();
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

        Txt_LevelInfo.gameObject.SetActive(true);

        Txt_LevelInfo.text = Levels[level].LevelCategory.ToString() + " - Level " + (Levels[level].LevelID+1);

        yield return new WaitForSeconds(0.5f);

        Txt_LevelInfo.gameObject.SetActive(false);
        StartCoroutine(ScreenManager.Instance.PlayEndTransition());
    }


    void LoadLevel(int level)
    {
        if (CurrentLevelInstance != null)
        {
            Destroy(CurrentLevelInstance);
        }

        BackgroundParalax.ResetParallax();

        CurrentLevelInstance = Instantiate(Levels[CurrentLevelID].LevelPrefab, LevelGameObject);
        CurrentLevelInstance.SetActive(true);

        Transform finishLine = CurrentLevelInstance.transform.Find("Finish"); 
                                                                                
        BackgroundParalax.SetFinishLine(finishLine);
        PlayerData playerData = SaveSystem.LoadPlayerData();

        BikeController.Instance.LoadPlayerBike(playerData.SELECTED_BIKE_ID);

        // Find the bike's starting position in the new level instance
        Transform bikeStartPosition = CurrentLevelInstance.transform.GetChild(0);
        GameManager.Instance.ResetLevelStats();
        // Set the bike's position to the starting position for the level
        GameManager.Instance.GAME_PlayerBike.SetActive(true);
        GameManager.Instance.GAME_PlayerBike.transform.SetPositionAndRotation(bikeStartPosition.position, bikeStartPosition.rotation);
        BikeController.Instance.PauseBike();
        ScreenManager.Instance.TweenMainMenu(false);
        GameManager.Instance.SetGameState(GameState.Starting);
        ScreenManager.Instance.TweenGameHUD(true);
        GameManager.Instance.ResetLevelStats();
        //CameraController.Instance.SwitchToGameCamera();
        BackgroundParalax.ResetParallax();
    }

}

// Other functions to handle level progression, scoring, etc.
