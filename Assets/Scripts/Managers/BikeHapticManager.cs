
using UnityEngine;
using Lofelt.NiceVibrations;
public class BikeHapticManager : MonoBehaviour
{
    public static BikeHapticManager Instance;
    
    [Header("Haptic Settings")]
    public bool HAPTIC_ON = false;
    public float MinimumPower = 0.1f;
    public float MaximumPower = 1.0f;
    private BikeController _bikeController; // Capitalize all instances
    private float _power;
    private float _targetPower;
    private float BuildUpTime = 3f;
    private float _startTime;


    private void Awake()
    {
        Instance = this;
        _bikeController = GetComponent<BikeController>();
    }

    private void Update()
    {
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
                _targetPower = (1 * speedPower + 2f * accelerationPower) / 1.8f;

                // Smoothly interpolate current power towards target power
                float t = (Time.time - _startTime) / BuildUpTime;
                _power = Mathf.Lerp(_power, _targetPower, t);

                // Send haptic feedback with a constant intensity based on the current power.
                HapticPatterns.PlayConstant(_power, _power, 0.05f);
                Debug.Log("Haptic: " + _power);
            }
            else
            {
                // Have haptic lerp from last val to 0 then stop.
                _startTime = Time.time; // Reset start time when the bike stops accelerating
                _power = 0;
                HapticController.Stop();
            }
        }
        
    }
}
