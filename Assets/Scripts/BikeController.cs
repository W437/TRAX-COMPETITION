using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BikeController : MonoBehaviour
{
    public static BikeController Instance;

    public WheelJoint2D backWheel;
    public WheelJoint2D frontWheel;
    public Transform backWheelTransform;
    public Transform frontWheelTransform;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    public float motorSpeed;
    public float maxTorque;
    public float downwardForce;
    public float accelerationTime;
    public float flipTorque;
    public float doublePressTime = 0.3f;
    public float flipDelay = 0.5f; // time in seconds to wait before flipping the bike
    public Collider2D bikeBody; // the bike's body collider

    WheelJoint2D wj;
    JointMotor2D mo;
    Rigidbody2D rb;

    private float currentMotorSpeed = 0f;
    private float accelerationStartTime;
    private float lastAirTime;

    // flip system
    public int flipCount = 0; // Flip counter
    private float lastZRotation = 0f;
    private float rotationCounter = 0;
    public SpriteRenderer bikeBodyRenderer; // assuming this is a SpriteRenderer, but it could also be a MeshRenderer or another type of renderer
    public SpriteRenderer frontWheelRenderer;
    public SpriteRenderer backWheelRenderer;
    public TrailRenderer bikeTrailRenderer;
    private Coroutine currentFlickerCoroutine = null;

    private Color originalBikeColor;
    private Color originalFrontWheelColor;
    private Color originalBackWheelColor;
    private Color originalTrailColor;

    private bool isWheelie = false;
    private float wheelieStartTime = 0f;
    private float totalWheelieTime = 0f;

    // Wheelie counter
    private float wheelieGracePeriod = 0.33f; // in seconds
    private float wheelieGraceEndTime;


    // Speed boost
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;
    public float normalMotorSpeed;


    // bike trail
    private float defaultTrailTime;



    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        wj = gameObject.GetComponents<WheelJoint2D>()[0];
        mo = new JointMotor2D();
        rb = GetComponent<Rigidbody2D>();
        lastZRotation = transform.eulerAngles.z;

        // Bike Colors
        originalBikeColor = bikeBodyRenderer.color;
        originalFrontWheelColor = frontWheelRenderer.color;
        originalBackWheelColor = backWheelRenderer.color;
        originalTrailColor = bikeTrailRenderer.startColor;

        // Bike Trail
        defaultTrailTime = bikeTrailRenderer.time;
    }

    private void Update()
    {
        bool isGrounded = IsGrounded();
        bool isMovingBackward = rb.velocity.x < 0; // Check if the bike is moving backward based on the rigidbody's velocity

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            accelerationStartTime = Time.time;
            wj.useMotor = true;
            //Debug.Log("Accelerating");
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (isGrounded && !isSpeedBoosted)
            {
                float elapsedTime = Time.time - accelerationStartTime;
                float progress = elapsedTime / accelerationTime;

                currentMotorSpeed = Mathf.Lerp(mo.motorSpeed, motorSpeed, progress);

                mo.motorSpeed = currentMotorSpeed;
                mo.maxMotorTorque = maxTorque;
                wj.motor = mo;
            }
            else
            {
                // Limit the flipping speed when in the air and pressing space
                float currentRotationSpeed = rb.angularVelocity;
                float maxRotationSpeed = 500f; // Adjust this value as needed

                if (Mathf.Abs(currentRotationSpeed) > maxRotationSpeed)
                {
                    rb.angularVelocity = Mathf.Sign(currentRotationSpeed) * maxRotationSpeed;
                }

                rb.AddTorque(flipTorque);
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            wj.useMotor = false;
            //Debug.Log("Stop Motor");
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }

        // Check if the bike body is in contact with the ground
        if (Physics2D.IsTouchingLayers(bikeBody, groundLayer))
        {
            if (Time.time - lastAirTime > flipDelay)
            {
                Flip();
            }
        }
        else
        {
            lastAirTime = Time.time;
        }

        // Flip Counter
        float rotationDiff = transform.eulerAngles.z - lastZRotation;
        if (rotationDiff > 180f) rotationDiff -= 360f;
        else if (rotationDiff < -180f) rotationDiff += 360f;

        rotationCounter += rotationDiff;
        if (Mathf.Abs(rotationCounter) >= 360f)
        {
            rotationCounter = 0;
            flipCount++;
            Debug.Log("Flip Count: " + flipCount);
        }

        lastZRotation = transform.eulerAngles.z;

        // Wheelie Counter
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);

        if (IsRearWheelGrounded() && hitFront.collider == null)
        {
            if (!isWheelie)
            {
                isWheelie = true;
                wheelieGraceEndTime = Time.time + wheelieGracePeriod;
            }
            else if (isWheelie && Time.time > wheelieGraceEndTime)
            {
                if (wheelieStartTime == 0)
                {
                    wheelieStartTime = Time.time;
                }
            }
        }
        else
        {
            if (isWheelie && wheelieStartTime != 0)
            {
                if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
                {
                    Debug.Log("Bike landed upside down. Wheelie not counted.");
                    wheelieStartTime = 0;
                }
                else
                {
                    isWheelie = false;
                    float wheelieTime = Time.time - wheelieStartTime;
                    if (wheelieTime > wheelieGracePeriod)
                    {
                        totalWheelieTime += wheelieTime;
                    }
                    Debug.Log("Wheelie time: " + FormatTime(wheelieTime));
                    Debug.Log("Total wheelie time: " + FormatTime(totalWheelieTime));
                    wheelieStartTime = 0;
                }
                wheelieGraceEndTime = 0;
                isWheelie = false;
            }
            else if (isWheelie && wheelieStartTime == 0)
            {
                isWheelie = false;
                wheelieGraceEndTime = 0;
            }
        }

        // Check for speed boost
        if (isSpeedBoosted && Time.time > speedBoostEndTime)
        {
            isSpeedBoosted = false;
            motorSpeed = currentMotorSpeed;
        }

        // Enable/disable the trail based on bike velocity
        if (isGrounded)
        {
            if (rb.velocity.x > 0 && !bikeTrailRenderer.emitting)
            {
                StartCoroutine(FadeTrailIn());
            }
            else if (rb.velocity.x <= 0 && bikeTrailRenderer.emitting)
            {
                StartCoroutine(FadeTrailOut());
            }
        }


        // Rest of the code...
    }

    private IEnumerator FadeTrailIn()
    {
        float elapsedTime = 0f;
        float initialTime = bikeTrailRenderer.time;
        // Enable the trail emission
        bikeTrailRenderer.emitting = true;

        // Gradually increase the trail time
        while (elapsedTime < 2f)
        {
            float time = Mathf.Lerp(initialTime, defaultTrailTime, elapsedTime);
            bikeTrailRenderer.time = time;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    private IEnumerator FadeTrailOut()
    {
        float elapsedTime = 0f;
        float initialTime = bikeTrailRenderer.time;

        // Gradually decrease the trail time
        while (elapsedTime < 0.5f)
        {
            float time = Mathf.Lerp(initialTime, 0f, elapsedTime);
            bikeTrailRenderer.time = time;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable the trail emission
        bikeTrailRenderer.emitting = false;
    }


    private bool IsGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitBack.collider != null || hitFront.collider != null;
    }

    private bool IsRearWheelGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitBack.collider != null;
    }

    private void Flip()
    {
        // If a previous flip is still in progress
        if (currentFlickerCoroutine != null)
        {
            StopCoroutine(currentFlickerCoroutine);
        }

        // Use a Raycast to detect the ground position
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, -Vector2.up, Mathf.Infinity, groundLayer);

        // Check if the ground was hit
        if (groundHit.collider != null)
        {
            // Set the position of the bike to be 1 unit above the ground
            transform.position = new Vector3(transform.position.x, groundHit.point.y + 1f, transform.position.z);

            // Calculate the angle of the slope where the bike is landing
            float slopeAngle = Mathf.Atan2(groundHit.normal.y, groundHit.normal.x) * Mathf.Rad2Deg;

            // Adjust the bike's rotation to match the slope
            transform.rotation = Quaternion.Euler(0, 0, slopeAngle - 90);
        }
        else
        {
            // Default behaviour if no ground was hit
            transform.position += new Vector3(0, 1f, 0);
        }

        // Start the respawn coroutine

        currentFlickerCoroutine = StartCoroutine(RespawnCoroutine());
    }


    private IEnumerator RespawnCoroutine()
    {
        // Duration variables
        float flickerDuration = 2f;
        float disableColliderDuration = 0.2f;

        // Save the time at the start of the method
        float startTime = Time.time;

        // Disable the bike's collider
        bikeBody.enabled = false;

        // Store the original color of the bike
        Color originalBikeColor = bikeBodyRenderer.color;
        Color originalFrontWheelColor = frontWheelRenderer.color;
        Color originalBackWheelColor = backWheelRenderer.color;
        Color originalTrailColor = bikeTrailRenderer.startColor;

        // Loop while the flicker duration hasn't passed
        while (Time.time - startTime < flickerDuration)
        {
            // Check if the disableColliderDuration has passed, if so re-enable the bike's collider
            if (!bikeBody.enabled && Time.time - startTime > disableColliderDuration)
            {
                bikeBody.enabled = true;
            }

            // Make the bike and its components transparent
            bikeBodyRenderer.color = new Color(originalBikeColor.r, originalBikeColor.g, originalBikeColor.b, 0.5f);
            frontWheelRenderer.color = new Color(originalFrontWheelColor.r, originalFrontWheelColor.g, originalFrontWheelColor.b, 0.5f);
            backWheelRenderer.color = new Color(originalBackWheelColor.r, originalBackWheelColor.g, originalBackWheelColor.b, 0.5f);
            bikeTrailRenderer.startColor = new Color(originalTrailColor.r, originalTrailColor.g, originalTrailColor.b, 0.5f);
            yield return new WaitForSeconds(0.1f);

            // Return to the original colors
            bikeBodyRenderer.color = originalBikeColor;
            frontWheelRenderer.color = originalFrontWheelColor;
            backWheelRenderer.color = originalBackWheelColor;
            bikeTrailRenderer.startColor = originalTrailColor;
            yield return new WaitForSeconds(0.1f);
        }

        // After the loop, ensure the bike's collider is re-enabled and the color is set back to its original state
        bikeBody.enabled = true;
        bikeBodyRenderer.color = originalBikeColor;
        frontWheelRenderer.color = originalFrontWheelColor;
        backWheelRenderer.color = originalBackWheelColor;
        bikeTrailRenderer.startColor = originalTrailColor;
    }




    private bool isBoosting = false; // Flag to track if the boost is active
    private float boostMotorSpeed; // The target motor speed during the boost

    public void ApplySpeedBoost(SpeedBoost.SpeedBoostData data)
    {
        if (!isBoosting)
        {
            isBoosting = true;
            boostMotorSpeed = motorSpeed + data.Amount;
            StartCoroutine(BoostCoroutine(data.Amount, data.Duration));
        }
    }

    private IEnumerator BoostCoroutine(float amount, float duration)
    {
        float elapsedTime = 0f;
        Vector2 initialUpVector = transform.up;
        Vector2 boostPosition = backWheelTransform.position; // Position to apply the boost force

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float currentMotorSpeed = Mathf.Lerp(motorSpeed, boostMotorSpeed, progress);

            // Calculate the direction of the boost
            Vector2 boostDirection = transform.right; // assuming the front of the bike is its right side

            // Apply the boost force to the bike's rigidbody at the boost position
            rb.AddForceAtPosition(boostDirection * amount * rb.mass, boostPosition, ForceMode2D.Force);

            // Counteract the leaning effect by applying an opposite force
            Vector2 currentUpVector = transform.up;
            Vector2 leanForce = -Vector2.Dot(currentUpVector, initialUpVector) * transform.forward * amount * rb.mass;
            rb.AddForceAtPosition(leanForce, boostPosition, ForceMode2D.Force);

            // Apply limited rotation force during the boost
            float rotationForceMultiplier = isSpeedBoosted ? 0.3f : 1f;
            rb.AddTorque(-flipTorque * rotationForceMultiplier);
            // HEY SOZO!
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBoosting = false;
        motorSpeed = boostMotorSpeed; // Set the motor speed to the boosted speed
        isSpeedBoosted = false;
    }

    public float GetWheelieTime()
    {
        return totalWheelieTime;
    }

    private void FixedUpdate()
    {
        // Apply a downward force to the car
        //rb.AddForce(Vector2.down * downwardForce, ForceMode2D.Force);
    }

    private string FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        return string.Format("{0:D2}:{1:D2}", timeSpan.Seconds, timeSpan.Milliseconds / 10);
    }
}
