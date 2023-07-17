using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Transform LevelGameObject;
    private BackgroundParalax BackgroundParalax;
    public TMPro.TextMeshProUGUI Txt_LevelInfo;
    public Level[] Levels;
    public int CurrentLevelID { get; private set; } = 0;
    [NonSerialized] public GameObject CurrentLevelInstance;


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


    void Start()
    {
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

    private IEnumerator StartLevelTransition(int level)
    {
        yield return ScreenManager.Instance.PlayStartTransition();
        LoadLevel(level);

        Txt_LevelInfo.gameObject.SetActive(true);

        Txt_LevelInfo.text = Levels[level].LevelCategory.ToString() + " - Level " + (Levels[level].LevelID + 1);

        yield return new WaitForSeconds(0.5f);

        Txt_LevelInfo.gameObject.SetActive(false);
        StartCoroutine(ScreenManager.Instance.PlayEndTransition());
    }

    private void LoadLevel(int level)
    {
        if (CurrentLevelInstance != null)
        {
            Destroy(CurrentLevelInstance);
        }

        CurrentLevelInstance = Instantiate(Levels[CurrentLevelID].LevelPrefab, LevelGameObject);
        CurrentLevelInstance.SetActive(true);

        // Find the Main Sprite game object in the current level
        GameObject mainSprite = CurrentLevelInstance.transform.Find("Main Sprite").gameObject;

        // Here you can pass your mainSprite to where you need it, like the BackgroundParalax script.
        // If the BackgroundParalax script has a method to set the main sprite, it might look like this:
        BackgroundParalax.Instance.SetMainSprite(mainSprite.GetComponent<SpriteShapeRenderer>());



        Transform finishLine = CurrentLevelInstance.transform.Find("_Finish");
        Transform startPosition = CurrentLevelInstance.transform.Find("_StartPos");

        BackgroundParalax.Instance.SetSunCalculations(finishLine, startPosition);
        PlayerData playerData = SaveSystem.LoadPlayerData();

        BikeController.Instance.LoadPlayerBike(playerData.SELECTED_BIKE_ID);

        // Find the bike's starting position in the new level instance
        Transform bikeStartPosition = CurrentLevelInstance.transform.GetChild(0);
        GameManager.Instance.ResetLevelStats();
        // Set the bike's position to the starting position for the level
        GameManager.Instance.InGAME_PlayerBike.SetActive(true);
        GameManager.Instance.InGAME_PlayerBike.transform.SetPositionAndRotation(bikeStartPosition.position, bikeStartPosition.rotation);
        BikeController.Instance.PauseBike();
        ScreenManager.Instance.TweenMainMenu(false);
        GameManager.Instance.SetGameState(GameState.Starting);
        ScreenManager.Instance.TweenGameHUD(true);
        GameManager.Instance.ResetLevelStats();
        CameraController.Instance.SwitchToGameCamera();
        BackgroundParalax.Instance.ResetParallax();

    }

}

