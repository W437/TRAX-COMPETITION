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
    public GameObject GAME_PlayerBike;
    public GameObject GAME_PlayerTrail;

    public PlayerData playerData;

    public bool firstLaunch = true;
    float sessionStartTime;
    public float CurrentPlayTime { get { return playerData.TOTAL_PLAYTIME + (Time.time - sessionStartTime); } }
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

    public AudioSource countdownAudioSource; 
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
        sessionStartTime = Time.time;
        initialCountdownTextPosition = countdownText.transform.position;
        //SaveSystem.ResetSaveFile();

    }


    void Update()
    {
        // Playtime tracker
        playerData.TOTAL_PLAYTIME = CurrentPlayTime;

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
                UpdateTimerText();
                UpdateFlipCountText();
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
        playerData.TOTAL_PLAYTIME += seconds;
    }

    public void SavePlayTime()
    {
        var _data = SaveSystem.LoadPlayerData();
        _data.TOTAL_PLAYTIME = CurrentPlayTime;
        sessionStartTime = Time.time; // Reset session start time after saving
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

    IEnumerator CountdownRoutine()
    {
        countdownTime = 3;
        // Reset the position of the countdownText to its initial position
        countdownText.transform.position = initialCountdownTextPosition;

        if (!countdownText.gameObject.activeSelf)
        {
            countdownText.gameObject.SetActive(true);
        }

        while (countdownTime > 0)
        {
            HapticPatterns.PlayConstant(0.35f, 0.35f, 0.35f); 
            countdownText.text = Mathf.CeilToInt(countdownTime).ToString();

            LeanTween.scale(countdownText.gameObject, Vector3.one * 1.5f, 0.1f)
                     .setOnComplete(() => LeanTween.scale(countdownText.gameObject, Vector3.one, 0.1f));

            countdownAudioSource.clip = countdownClip;
            countdownAudioSource.Play();

            yield return new WaitForSeconds(1.0f);

            countdownTime--;
        }

        countdownAudioSource.clip = goClip;
        countdownAudioSource.Play();
        HapticPatterns.PlayConstant(0.50f, 0.65f, 0.65f); 
        countdownText.text = "GO!";

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


    public void SetGameState(GameState newState)
    {
        var _bikeController = BikeController.Instance;
        gameState = newState;

        if (gameState == GameState.Paused)
        {
            _bikeController.StopSavingDistance();
        }
        else if (gameState == GameState.Playing)
        {
            _bikeController.CAN_CONTROL = true;
            if (BikeController.Instance.saveDistanceCoroutine == null)
            {
                _bikeController.saveDistanceCoroutine = _bikeController.SaveDistanceEveryFewSeconds(30.0f);
                StartCoroutine(_bikeController.saveDistanceCoroutine);
            }
        }
        else if (gameState == GameState.Menu)
        {

            // Delete the previous level instance if it exists
            if (LevelManager.Instance.CurrentLevelInstance != null)
            {
                Destroy(LevelManager.Instance.CurrentLevelInstance);
            }

            if (GameManager.Instance.GAME_PlayerBike != null && GameManager.Instance.GAME_PlayerBike.activeSelf)
            {
                GameManager.Instance.GAME_PlayerBike.SetActive(false);
            }

            CameraController.Instance.SwitchToMenuCamera();
            ScreenManager.Instance.TweenMainMenu(true);

            if(!ScreenManager.Instance.GameLogo.activeSelf)
                ScreenManager.Instance.TweenGameLogo(true);
        }
        else if (gameState == GameState.Starting)
        {
            if (!GAME_PlayerBike.activeSelf)
            {
                GAME_PlayerBike.SetActive(true);
            }

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
}