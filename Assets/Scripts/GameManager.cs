using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;
    public TMPro.TextMeshProUGUI faultCountText;

    private float totalWheelieTime = 0f;
    public GameState gameState;

    private float timer; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = 0f;
        SetGameState(GameState.Playing);
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
        }


    }

    private void UpdateTimerText()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        string timerString = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
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

        string wheelieTimeString = string.Format("{0:D2}.{1:D3}", totalWheelieTimeSeconds, totalWheelieTimeMilliseconds);
        wheelieTimeText.text = "" + wheelieTimeString + "s";
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
        Paused,
        Playing,
        Menu
    }

    public void SetGameState(GameState newState)
    {
        var gameState = newState;
        if (gameState == GameState.Paused)
        {
        }
        else if (gameState == GameState.Playing)
        {

        }
        else if (gameState == GameState.Menu)
        {
            // Logic to go back to menu
            // Possibly load a menu scene or enable menu UI
        }
    }

    public void StartLevel(int level)
    {
        MenuController.Instance.Menu.SetActive(false);
        SetGameState(GameState.Playing);
    }




}

