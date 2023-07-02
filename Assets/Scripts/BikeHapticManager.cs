
using UnityEngine;
using Lofelt.NiceVibrations;
public class BikeHapticManager : MonoBehaviour
{
    public static BikeHapticManager Instance;
    public bool HAPTIC_ON = false;
    [Header("Haptic Settings")]
    public float MinimumPower = 0.1f;
    public float MaximumPower = 1.0f;
    public float AccelerationIncreaseSpeed = 0.1f; // Adjust as needed
    public float DecelerationDecreaseSpeed = 0.2f; // Adjust as needed

    private BikeController _bikeController;
    private float _power;
    private float _targetPower;
    public float BuildUpTime = 3f;
    private float _startTime;


    private void Awake()
    {
        Instance = this;
        _bikeController = GetComponent<BikeController>();
    }

    private void Update()
    {
        //Debug.Log("Haptic: " + _power);
        if(HAPTIC_ON && (GameManager.Instance.gameState == GameManager.GameState.Playing))
        {
            if (_bikeController.isAccelerating && _bikeController.IsGrounded())
            {
                // Get bike speed and acceleration
                float bikeSpeed = _bikeController.GetBikeSpeed();
                float bikeAcceleration = _bikeController.GetBikeAcceleration();

                // Calculate power values for both speed and acceleration
                float speedPower = Mathf.Clamp(bikeSpeed / 10f, MinimumPower, MaximumPower);
                float accelerationPower = Mathf.Clamp(Mathf.Abs(bikeAcceleration) / 10f, MinimumPower, MaximumPower);

                // Weighted average of the two power values giving more weight to speed
                _targetPower = (2 * speedPower + 0.5f * accelerationPower) / 2.5f;

                // Smoothly interpolate current power towards target power
                float t = (Time.time - _startTime) / BuildUpTime;
                _power = Mathf.Lerp(_power, _targetPower, t);

                // Use this line to send haptic feedback with a constant intensity based on the current power.
                HapticPatterns.PlayConstant(_power, _power, 0f);
                Debug.Log("Haptic: " + _power);
            }
            else
            {
                _startTime = Time.time; // Reset start time when the bike stops accelerating
                _power = 0; // Set power to 0 when the bike is not accelerating
                HapticController.Stop();
            }
        }
        
    }
}
