using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float SpeedBoostAmount;
    public float SpeedBoostDuration;
    bool flag = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!flag && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boost Col");
            BikeController.Instance.ApplySpeedBoost(new SpeedBoostData(SpeedBoostAmount, SpeedBoostDuration));
            flag = true;
        }
    }


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