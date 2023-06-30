using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bike[] BikeList;
    public Trail[] TrailList;

    // for level load
    public GameObject GamePlayerBikeInstance;
    public GameObject GamePlayerTrailInstance;
    // level load




    public PlayerData playerData;

    public bool firstLaunch = true;
    public GameObject playerObjectParent;
    public LayerMask groundLayer;

    [Header("Game HUD")]

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI countdownText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;
    public TMPro.TextMeshProUGUI faultCountText;

    public BackgroundParalax backgroundParalax;



    float totalWheelieTime = 0f;
    public GameState gameState;

    float countdownTime;
    public float LevelTimer { get; private set; }


    [Header("Sound Effects")]

    public AudioSource countdownAudioSource;  // for countdown and Go sounds
    public AudioClip countdownClip;
    public AudioClip goClip;

    Vector2 initialCountdownTextPosition;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;

        playerData = SaveSystem.LoadPlayerData();

        SetGameState(GameState.Menu);

    }

    void Start()
    {
        initialCountdownTextPosition = countdownText.transform.position;
        //SaveSystem.ResetSaveFile();
        PrintAllPlayerData();
    }

    public void PrintAllPlayerData()
    {
        PlayerData playerData = GameManager.Instance.GetPlayerData();
        Debug.Log("To print data;");
        foreach (var levelStat in playerData.levelStatsDictionary)
        {
            Debug.Log($"Level: {levelStat.Key}, Time: {levelStat.Value.time}");
        }
    }


    void Update()
    {
        // Manage Game State

        switch (gameState)
        {
            case GameState.Paused:
                // Pause game logic here
                break;

            case GameState.Playing:
                LevelTimer += Time.deltaTime;
                UpdateTimerText();
                UpdateFlipCountText();
                UpdateWheelieTimeText();
                break;

            case GameState.Menu:
                // Menu display logic here
                break;

            case GameState.Starting:
                // The countdown logic has been moved to SetGameState(), so nothing is needed here
                break;
        }
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }

    public void ResetLevelStats()
    {
        LevelTimer = 0f;
        wheelieTimeText.text = "0";
        timerText.text = "0:00:00";
        flipCountText.text = "0";
        faultCountText.text = "0";
        totalWheelieTime = 0;
        BikeController.Instance.wheeliePoints = 0;
        BikeController.Instance.faults = 0;
        BikeController.Instance.flipCount = 0;
        BikeController.Instance.totalWheelieTime = 0;
    }

    void UpdateTimerText()
    {
        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(LevelTimer);
        string timerString = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);
        timerText.text = timerString;
    }


    void UpdateFlipCountText()
    {
        int _flipCount = BikeController.Instance.flipCount;
        flipCountText.text = "" + _flipCount;
    }

    public void UpdateFaultCountText()
    {
        // Get the flip count from the BikeController script
        int _faultCount = BikeController.Instance.GetFaultCount();
        faultCountText.text = "" + _faultCount;
    }

    public void AccumulateWheelieTime(float wheelieTime)
    {
        totalWheelieTime += wheelieTime;
        UpdateWheelieTimeText();
    }

    void UpdateWheelieTimeText()
    {
        int _totalWheelieTimeSeconds = (int)totalWheelieTime;
        int _totalWheelieTimeMilliseconds = (int)((totalWheelieTime - _totalWheelieTimeSeconds) * 1000);

        string _wheelieTimeString;

        if (_totalWheelieTimeMilliseconds >= 100)
        {
            _wheelieTimeString = string.Format("{0:D2}.{1:D2}", _totalWheelieTimeSeconds, _totalWheelieTimeMilliseconds / 10);
        }
        else
        {
            _wheelieTimeString = string.Format("{0:D2}.{1:00}", _totalWheelieTimeSeconds, _totalWheelieTimeMilliseconds);
        }

        wheelieTimeText.text = "" + _wheelieTimeString + "";
    }

    public void PauseGame()
    {
        SetGameState(GameState.Paused);
        ScreenManager.Instance.TweenPauseGame(true);
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
        ScreenManager.Instance.TweenPauseGame(false);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    void LevelFinish()
    {

    }
    
    void OnGameOver()
    {

    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Starting
    }


    IEnumerator CountdownRoutine()
    {
        countdownTime = 3;

        // Reset the position of the countdownText to its initial position
        countdownText.transform.position = initialCountdownTextPosition;

        // Make the game object active if it's initially inactive
        if (!countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(true);
        }

        while (countdownTime > 0)
        {
            countdownText.text = Mathf.CeilToInt(countdownTime).ToString();

            // Scale animation using LeanTween
            LeanTween.scale(countdownText.gameObject, Vector3.one * 1.5f, 0.1f)
                     .setOnComplete(() => LeanTween.scale(countdownText.gameObject, Vector3.one, 0.1f));

            countdownAudioSource.clip = countdownClip;
            countdownAudioSource.Play();

            yield return new WaitForSeconds(1.0f);

            countdownTime--;
        }

        countdownAudioSource.clip = goClip;
        countdownAudioSource.Play();
        countdownText.text = "GO!";

        // Scale animation using LeanTween
        LeanTween.scale(countdownText.gameObject, Vector3.one * 1.5f, 0.1f)
                 .setOnComplete(() => LeanTween.scale(countdownText.gameObject, Vector3.one, 0.1f));

        yield return new WaitForSeconds(0.5f);

        // Push animation using LeanTween
        LeanTween.moveY(countdownText.rectTransform, -Screen.height, 0.5f)
                 .setOnComplete(() =>
                 {
                     countdownText.gameObject.SetActive(false);
                 });

        // Start game
        BikeController.Instance.ResumeBike();
        SetGameState(GameState.Playing);
    }


    public void SetGameState(GameState newState)
    {
        gameState = newState;
        if (gameState == GameState.Paused)
        {

        }
        else if (gameState == GameState.Playing)
        {

        }
        else if (gameState == GameState.Menu)
        {
            // Delete the previous level instance if it exists
            if (LevelManager.Instance.currentLevelInstance != null)
            {
                Destroy(LevelManager.Instance.currentLevelInstance);
            }

            if (GameManager.Instance.GamePlayerBikeInstance != null && GameManager.Instance.GamePlayerBikeInstance.activeSelf)
            {
                GameManager.Instance.GamePlayerBikeInstance.SetActive(false);
            }

            //ShopManager.Instance.CurrentPlayerBike.transform.position = new Vector2(0, 0);

            //ScreenManager.Instance.RB_MenuPlatform.position = new Vector2(-4, 0);
            //ScreenManager.Instance.RB_MenuBike.isKinematic = true;

            CameraController.Instance.SwitchToMenuCamera();

            // Set the bike's position relative to the platform
            //Vector2 platformPosition = ScreenManager.Instance.MenuPlatform.transform.position;
            //Vector2 bikePosition = new Vector2(platformPosition.x, 0);
            //ScreenManager.Instance.RB_MenuBike.position = bikePosition;
            //ScreenManager.Instance.RB_MenuBike.transform.rotation = Quaternion.identity;
            //ScreenManager.Instance.RB_MenuBike.rotation = 0;
            ScreenManager.Instance.TweenMainMenu(true);

            // Optional: Reset any other variables or states specific to the menu screen
        }
        else if (gameState == GameState.Starting)
        {
            //BikeController.Instance.PauseBike();
            //ScreenManager.Instance.RB_MenuBike.isKinematic = true;

            if (!GamePlayerBikeInstance.activeSelf)
            {
                GamePlayerBikeInstance.SetActive(true);
            }

            //ScreenManager.Instance.RB_MenuBike.isKinematic = true;
            ScreenManager.Instance.MenuPlatform.SetActive(false);
            //ScreenManager.Instance.MenuBike.SetActive(false);
            CameraController.Instance.SwitchToGameCamera();
            // Call the Countdown Coroutine when the GameState is set to Starting
            StartCoroutine(CountdownRoutine());
        }
    }
}