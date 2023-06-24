using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game HUD")]
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI countdownText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;
    public TMPro.TextMeshProUGUI faultCountText;

    private float totalWheelieTime = 0f;
    public GameState gameState;

    private float countdownTime;
    private float timer;

    [Header("Sound Effects")]

    public AudioSource countdownAudioSource;  // for countdown and Go sounds
    public AudioClip countdownClip;
    public AudioClip goClip;

    private Vector3 initialCountdownTextPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetLevelStats();
        SetGameState(GameState.Menu);
        initialCountdownTextPosition = countdownText.transform.position;
    }


    private void Update()
    {
        // Manage Game State

        switch (gameState)
        {
            case GameState.Paused:
                // Pause game logic here
                break;

            case GameState.Playing:
                timer += Time.deltaTime;
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

    public void ResetLevelStats()
    {
        timer = 0f;
        wheelieTimeText.text = "0";
        timerText.text = "00:00:00";
        flipCountText.text = "0";
        faultCountText.text = "0";
        totalWheelieTime = 0;
        BikeController.Instance.wheeliePoints = 0;
        BikeController.Instance.faults = 0;
        BikeController.Instance.flipCount = 0;
        BikeController.Instance.totalWheelieTime = 0;
    }

    private void UpdateTimerText()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        int millisecondFirstTwoDigits = timeSpan.Milliseconds / 10;
        string timerString = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, millisecondFirstTwoDigits);
        timerText.text = "" + timerString;
    }

    private void UpdateFlipCountText()
    {
        // Get the flip count from the BikeController script
        int flipCount = BikeController.Instance.flipCount;
        flipCountText.text = "" + flipCount;
    }


    public void UpdateFaultCountText()
    {
        // Get the flip count from the BikeController script
        int faultCount = BikeController.Instance.GetFaultCount();
        faultCountText.text = "" + faultCount;
    }

    public void AccumulateWheelieTime(float wheelieTime)
    {
        totalWheelieTime += wheelieTime;
        UpdateWheelieTimeText();
    }

    private void UpdateWheelieTimeText()
    {
        int totalWheelieTimeSeconds = (int)totalWheelieTime;
        int totalWheelieTimeMilliseconds = (int)((totalWheelieTime - totalWheelieTimeSeconds) * 1000);

        string wheelieTimeString;

        if (totalWheelieTimeMilliseconds >= 100)
        {
            wheelieTimeString = string.Format("{0:D2}.{1:D2}", totalWheelieTimeSeconds, totalWheelieTimeMilliseconds / 10);
        }
        else
        {
            wheelieTimeString = string.Format("{0:D2}.{1:00}", totalWheelieTimeSeconds, totalWheelieTimeMilliseconds);
        }

        wheelieTimeText.text = "" + wheelieTimeString + "";
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

    private void LevelFinish()
    {

    }
    
    private void OnGameOver()
    {

    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Starting
    }


    private IEnumerator CountdownRoutine()
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
        BikeController.Instance.RB_Bike.velocity = Vector2.zero;
        BikeController.Instance.RB_Bike.angularVelocity = 0f;
        BikeController.Instance.accelerationTimer = 0f;
        BikeController.Instance.shouldMove = false;
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
            BikeController.Instance.RB_Bike.position = new Vector2(-131.52f, -8.24f);
            BikeController.Instance.accelerationTimer = 0;
            BikeController.Instance.shouldMove = true;
            ScreenManager.Instance.TweenMainMenu(true);
            //BikeController.Instance.PauseBike();
        }
        else if (gameState == GameState.Starting)
        {
            //BikeController.Instance.PauseBike();

            // Call the Countdown Coroutine when the GameState is set to Starting
            StartCoroutine(CountdownRoutine());
        }
    }





}
