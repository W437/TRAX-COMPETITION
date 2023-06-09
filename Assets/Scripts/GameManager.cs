using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;

    private float totalWheelieTime = 0f;

    private float timer;
    private BikeController bikeController; // Reference to the BikeController component

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Reset the timer
        timer = 0f;

        // Get the BikeController component
        bikeController = BikeController.Instance;
    }

    private void Update()
    {
        // Update the timer
        timer += Time.deltaTime;
        UpdateTimerText();

        // Update the flip count
        UpdateFlipCountText();

        // Update the wheelie time
        UpdateWheelieTimeText();
    }

    private void UpdateTimerText()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        string timerString = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        timerText.text = "Time: " + timerString;
    }

    private void UpdateFlipCountText()
    {
        // Get the flip count from the BikeController script
        int flipCount = bikeController.flipCount;
        flipCountText.text = "Flips: " + flipCount;
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
        wheelieTimeText.text = "Total Wheelie Time: " + wheelieTimeString;
    }




}
