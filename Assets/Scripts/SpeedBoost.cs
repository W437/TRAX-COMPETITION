using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float speedBoostAmount; // the amount of speed to add
    public float speedBoostDuration; // the duration of the speed boost in seconds
    bool flag = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!flag && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boost Col");
            BikeController.Instance.ApplySpeedBoost(new SpeedBoostData(speedBoostAmount, speedBoostDuration));
            flag = true;
        }
    }


    // Define SpeedBoostData struct to hold speed boost parameters
    public struct SpeedBoostData
    {
        public float Amount;
        public float Duration;

        public SpeedBoostData(float amount, float duration)
        {
            Amount = amount;
            Duration = duration;
        }
    }
}