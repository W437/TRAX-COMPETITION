using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI flipCountText;
    public TMPro.TextMeshProUGUI wheelieTimeText;

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

    private void UpdateWheelieTimeText()
    {
        // Get the wheelie time from the BikeController script
        float wheelieTime = bikeController.GetWheelieTime();
        TimeSpan timeSpan = TimeSpan.FromSeconds(wheelieTime);
        string wheelieTimeString = string.Format("{0:D2}:{1:D2}", timeSpan.Seconds, timeSpan.Milliseconds / 10);
        wheelieTimeText.text = "Wheelie Time: " + wheelieTimeString;
    }

}
