using Cinemachine;
using Lofelt.NiceVibrations;
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
    [NonSerialized] public GameObject InGAME_PlayerBike;
    [NonSerialized] public GameObject InGAME_PlayerTrail;

    public PlayerData PlayerData;

    public bool firstLaunch = true;
    private float sessionStartTime;
    public float CurrentPlayTime { get { return PlayerData.TOTAL_PLAYTIME + (Time.time - sessionStartTime); } }
    public GameObject playerObjectParent;
    public LayerMask GroundLayer;
    private float totalWheelieTime = 0f;
    public GameState gameState;

    private float countdownTime;


    [Header("Game HUD")]
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI countdownText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;
    public TMPro.TextMeshProUGUI faultCountText;
    private Vector2 _initialCountdownTextPosition;
    public float LevelTimer { get; private set; }


    [Header("Sound Effects")]
    public AudioSource countdownAudioSource; 
    public AudioClip countdownClip;
    public AudioClip goClip;


    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        PlayerData = SaveSystem.LoadPlayerData();
        SetGameState(GameState.Menu);
        
        sessionStartTime = Time.time;
        _initialCountdownTextPosition = countdownText.transform.position;
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }

        // Manage Game State
        switch (gameState)
        {
            case GameState.Paused:

                break;

            case GameState.Playing:
                LevelTimer += Time.deltaTime;
                UpdateGameTimerText();
                UpdateGameFlipCountText();
                UpdateWheeliePointsText();
                break;

            case GameState.Menu:

                break;

            case GameState.Starting:

                break;
        }
    }


    public void AddPlayTime(float seconds)
    {
        PlayerData.TOTAL_PLAYTIME += seconds;
    }

    public void SavePlaytimeAndDistance()
    {
        var _data = SaveSystem.LoadPlayerData();
        _data.TOTAL_PLAYTIME = CurrentPlayTime;
        _data.TOTAL_DISTANCE += BikeController.Instance.GetTotalDistanceInKilometers();
        //sessionStartTime = Time.time; // Reset session start time after saving
        SaveSystem.SavePlayerData(_data);
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

    void UpdateGameTimerText()
    {
        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(LevelTimer);
        string timerString = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);
        timerText.text = timerString;
    }

    void UpdateGameFlipCountText()
    {
        int _flipCount = BikeController.Instance.flipCount;
        flipCountText.text = "" + _flipCount;
    }

    public void UpdateGameFaultCountText()
    {
        int _faultCount = BikeController.Instance.GetFaultCount();
        faultCountText.text = "" + _faultCount;
    }

    public void UpdateWheeliePoints(float wheelieTime)
    {
        totalWheelieTime += wheelieTime;
        UpdateWheeliePointsText();
    }

    void UpdateWheeliePointsText()
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

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Finished,
        Starting
    }

    public void SetGameState(GameState newState)
    {
        var _bikeController = BikeController.Instance;
        gameState = newState;

        if (gameState == GameState.Paused)
        {

        }
        else if (gameState == GameState.Playing)
        {
            BackgroundParalax.Instance.ResetParallax();
            _bikeController.CAN_CONTROL = true;
        }
        else if (gameState == GameState.Menu)
        {
            BackgroundParalax.Instance.ResetParallax();
            if (ScreenManager.Instance != null)
                ScreenManager.Instance.RefreshTextValuesFromPlayerData();
            // Delete the previous level instance if it exists
            if (LevelManager.Instance.CurrentLevelInstance != null)
            {
                Destroy(LevelManager.Instance.CurrentLevelInstance);
            }

            if (InGAME_PlayerBike != null && InGAME_PlayerBike.activeSelf)
            {
                InGAME_PlayerBike.SetActive(false);
            }

            CameraController.Instance.SwitchToMenuCamera();
            ScreenManager.Instance.TweenMainMenu(true);

            if(!ScreenManager.Instance.MENU_GameLogo.activeSelf)
                ScreenManager.Instance.TweenGameLogo(true);
        }
        else if (gameState == GameState.Starting)
        {
            BackgroundParalax.Instance.ResetParallax();

            if (!InGAME_PlayerBike.activeSelf)
            {
                InGAME_PlayerBike.SetActive(true);
            }

            ScreenManager.Instance.ResetTrophiesDefaultScale();
            ScreenManager.Instance.PlayerMenuBikeRb.isKinematic = true;
            ScreenManager.Instance.MenuPlatformObject.SetActive(false);
            ScreenManager.Instance.PlayerMenuBike.SetActive(false);
            CameraController.Instance.SwitchToGameCamera();

            // Call the Countdown
            StartCoroutine(CountdownRoutine());
        }
        else if (gameState == GameState.Finished)
        {
            _bikeController.CAN_CONTROL = false;
        }
    }

    IEnumerator CountdownRoutine()
    {
        countdownTime = 3;
        // Reset the position of the countdownText to its initial position
        countdownText.transform.position = _initialCountdownTextPosition;

        if (!countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(true);
        }

        while (countdownTime > 0)
        {
            countdownText.text = Mathf.CeilToInt(countdownTime).ToString();

            HapticPatterns.PlayConstant(0.35f, 0.35f, 0.25f); 
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
        HapticPatterns.PlayConstant(0.50f, 0.65f, 0.65f); 

        LeanTween.scale(countdownText.gameObject, Vector3.one * 1.5f, 0.1f)
                 .setOnComplete(() => LeanTween.scale(countdownText.gameObject, Vector3.one, 0.1f));

        yield return new WaitForSeconds(0.5f);

        LeanTween.moveY(countdownText.rectTransform, -Screen.height, 0.5f)
        .setOnComplete(() =>
        {
            countdownText.gameObject.SetActive(false);
        });

        // Start game
        BikeController.Instance.ResumeBike();
        SetGameState(GameState.Playing);
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameState == GameState.Playing)
        {
            ScreenManager.Instance.PauseGame();

        }
        else
        {
            // Game regained focus
            // Add your code here for when the game is back in focus (e.g., resume the game)
        }
    }

    void OnApplicationQuit()
    {
        var _data = SaveSystem.LoadPlayerData();
        SavePlaytimeAndDistance();
        SaveSystem.SavePlayerData(_data);
    }

}