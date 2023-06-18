using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BikeController;
using static GameManager;

public class BikeController : MonoBehaviour
{
    // Singleton Instance
    public static BikeController Instance;
    public MMFeedbacks StatsJuice;

    // External References
    [SerializeField] private WheelJoint2D backWheel;
    [SerializeField] private WheelJoint2D frontWheel;
    [SerializeField] private CircleCollider2D rearWheelCollider;
    [SerializeField] private Transform backWheelTransform;
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private CapsuleCollider2D bikeBody;
    [SerializeField] private SpriteRenderer bikeBodyRenderer;
    [SerializeField] private SpriteRenderer frontWheelRenderer;
    [SerializeField] private SpriteRenderer backWheelRenderer;
    [SerializeField] private TrailRenderer bikeTrailRenderer;

    // Bike Properties
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float motorSpeed;
    [SerializeField] private float maxTorque = 5f;
    [SerializeField] private float downwardForce;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private float initialMaxTorque = 0.5f; // Starting torque
    private float currentMotorSpeed = 0f;
    private float initialMotorSpeed;
    private float accelerationStartTime;

    // PlayerPrefs Save Data
    float bestTime = 0;
    int totalFlips = 0;
    int totalWheelieTime = 0;
    float bestSingleWheelieTime = 0;
    int faults = 0;
    int totalfaults = 0;

    // Physics System
    private WheelJoint2D wj;
    private JointMotor2D mo;
    private Rigidbody2D rb;
    private float lastAirTime;

    // Double Mouse Press System
    [SerializeField] private float doublePressTime = 0.3f;
    private float lastClickTime = 0f;
    private int mouseClicks = 0;
    private float mouseClickTimer = 0.0f;
    private bool isDoubleMousePressed = false;
    private Vector2 doubleClickForceDirection;
    private Quaternion doubleClickRotation;
    private bool isBeingPushedForward = false;
    private float originalAngularDrag;
    private Coroutine rotateBikeCoroutine = null;
    private float flickStartTime;
     
    // Flip System
    public int flipCount = 0; // Flip counter
    [SerializeField] private float flipDelay = 0.5f; // time in seconds to wait before flipping the bike
    [SerializeField] private float flipTorque;
    private float lastZRotation = 0f;
    private float rotationCounter = 0;
    private int internalFlipCount = 0;
    private bool hasLanded = false;
    [SerializeField] private float maxAirRotationSpeed = 650f; // Adjust this value as needed
    private Coroutine currentFlickerCoroutine = null;
    bool hasBeenUpsideDown = false;



    // Wheelie System
    private float wheelieGracePeriod = 0.13f; // in seconds
    private float wheelieGraceEndTime;
    private bool isBodyTouchingGround = false;
    private bool isWheelie = false;
    private float wheelieStartTime = 0f;

    // Speed Boost System
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;
    private float normalMotorSpeed;
    private bool isBoosting = false; // Flag to track if the boost is active
    private float boostMotorSpeed; // The target motor speed during the boost


    // Bike Trail System
    private float defaultTrailTime;

    // Visuals
    private Color originalBikeColor;
    private Color originalFrontWheelColor;
    private Color originalBackWheelColor;
    private Color originalTrailColor;




    private float maxAirHeight;
    // ----- VAR END ----- //

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
        rb.isKinematic = true;

        // Bike Trail
        defaultTrailTime = bikeTrailRenderer.time;

        // Store the original color of the bike
        originalBikeColor = bikeBodyRenderer.color;
        originalFrontWheelColor = frontWheelRenderer.color;
        originalBackWheelColor = backWheelRenderer.color;
        originalTrailColor = bikeTrailRenderer.startColor;
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.gameState == GameState.Playing)
        {
            if (isDoubleMousePressed)
            {
                rb.AddForce(doubleClickForceDirection * 2.5f, ForceMode2D.Impulse);
                isDoubleMousePressed = false;
                isBeingPushedForward = true;
                flickStartTime = Time.time; // Set the flick start time

                if (rotateBikeCoroutine != null)
                {
                    StopCoroutine(rotateBikeCoroutine);
                }
                rotateBikeCoroutine = StartCoroutine(RotateBikeToFaceForward(0.5f));
            }
            else if (isBeingPushedForward)
            {
                // Check if 0.3 seconds has passed since the start of the forward push
                if (Time.time >= flickStartTime + 0.5f)
                {
                    isBeingPushedForward = false; // Stop pushing forward
                    rb.angularDrag = originalAngularDrag; // reset angularDrag to its original value
                    rb.angularVelocity = 0f;
                    rb.constraints = RigidbodyConstraints2D.None;

                }
            }
        }
    }

    private void Update()
    {
        if(GameManager.Instance.gameState == GameState.Playing)
        {
            rb.isKinematic = false;
            maxAirHeight = Mathf.Max(maxAirHeight, transform.position.y);
            bool _isGrounded = IsGrounded();

            // Detect double click
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                mouseClicks++;
                if (mouseClicks == 1)
                    mouseClickTimer = Time.time;
            }

            if (mouseClicks > 0 && Time.time - mouseClickTimer > doublePressTime)
                mouseClicks = 0;

            if (mouseClicks == 2)
            {
                mouseClicks = 0;
                if (!_isGrounded) // We only care if the bike is in the air
                {
                    // Save the direction that the bike is facing (assuming that the bike is facing its local up direction)
                    doubleClickForceDirection = Vector2.right;
                    doubleClickRotation = transform.rotation;
                    isDoubleMousePressed = true;
                    isBeingPushedForward = true;
                    originalAngularDrag = rb.angularDrag;
                    rb.angularDrag = 1f; // a high value to strongly resist rotation
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                }
            }


            //-----------------------------------  Input

            if (Input.GetKey(KeyCode.Mouse0))
            {
                HandleBike();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                accelerationStartTime = Time.time;
                wj.useMotor = true;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                wj.useMotor = false;
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }

            //--------------------

            CheckGroundContact();

            HandleFlips();

            HandleWheelie();

            CheckSpeedBoost();

            HandleTrail();
        }

    }


    void HandleBike()
    {
        bool isGrounded = IsGrounded();
        if (isBeingPushedForward) // Add this condition
        {
            //rb.angularVelocity = 0; // Reset angular velocity
            // Don't apply torque while pushing forward
        }
        if (isGrounded && !isSpeedBoosted)
        {
            float elapsedTime = Time.time - accelerationStartTime;
            float progress = elapsedTime / accelerationTime;

            float easedProgress = progress * progress; // Quadratic easing
            //float easedProgress = progress * progress * progress; // Cubic easing

            mo.maxMotorTorque = Mathf.Lerp(initialMaxTorque, maxTorque, easedProgress);
            mo.motorSpeed = motorSpeed;
            wj.motor = mo;
        }



        else if (!isBeingPushedForward)
        {
            // Limit the flipping speed when in the air and pressing space
            float currentRotationSpeed = rb.angularVelocity;

            if (Mathf.Abs(currentRotationSpeed) > maxAirRotationSpeed)
            {
                rb.angularVelocity = Mathf.Sign(currentRotationSpeed) * maxAirRotationSpeed;
            }

            rb.AddTorque(flipTorque);
        }
    }



    void CheckGroundContact()
    {
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
    }



    void HandleFlips()
    {
        bool isGrounded = IsGrounded();
        if (hasLanded)
        {
            hasLanded = false;
            if (internalFlipCount > 0)
            {
                flipCount += internalFlipCount;
                Debug.Log("Final Flip Count: " + flipCount);
                PlayerPrefs.SetInt("FLIPS", flipCount);
                PlayerPrefs.Save();

                float _mostFlipCount = PlayerPrefs.GetInt("MOST_FLIP_COUNT");

                if (flipCount > _mostFlipCount)
                {
                    PlayerPrefs.SetInt("MOST_FLIP_COUNT", flipCount);
                    PlayerPrefs.Save();
                }


                internalFlipCount = 0;
            }
        }
        else if (!isBodyTouchingGround || (isBodyTouchingGround && !isGrounded))
        {
            float rotationDiff = transform.eulerAngles.z - lastZRotation;
            if (rotationDiff > 180f) rotationDiff -= 360f;
            else if (rotationDiff < -180f) rotationDiff += 360f;

            rotationCounter += rotationDiff;

            // Check if the bike has been upside down
            if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
            {
                hasBeenUpsideDown = true;
            }

            // Only count a flip if the bike has been upside down and completed a full rotation
            if (hasBeenUpsideDown && Mathf.Abs(rotationCounter) >= 360f)
            {
                rotationCounter = 0;
                internalFlipCount++;
                StatsJuice.PlayFeedbacks();
                Debug.Log("Intermediate Flip Count: " + internalFlipCount);
                hasBeenUpsideDown = false; // Reset for the next flip
            }
        }

        lastZRotation = transform.eulerAngles.z;
    }



    void HandleWheelie()
    {
        // Wheelie System (Resets if body collides w/ ground, records if in progress and loses contact w/ ground)

        if (isWheelie && wheelieStartTime != 0 && isBodyTouchingGround)
        {
            //Debug.Log("Bike body hit the ground. Wheelie not counted.");
            isWheelie = false;
            wheelieStartTime = 0;
            isBodyTouchingGround = false; // Reset the flag
        }

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
            bool hasJumped = (hitFront.collider == null && !IsRearWheelGrounded());

            RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
            if (isWheelie && wheelieStartTime != 0)
            {
                // Check if the bike lands on two wheels or has jumped while wheelie in progress
                if (hitBack.collider != null || hasJumped)
                {
                    isWheelie = false;
                    float wheelieTime = Time.time - wheelieStartTime;
                    if (wheelieTime > wheelieGracePeriod)
                    {
                        //Debug.Log("Wheelie time: " + FormatTime(wheelieTime));
                        PlayerPrefs.SetFloat("WHEELIE_TIME", wheelieTime);
                        PlayerPrefs.Save();

                        float _bestWheelieTime = PlayerPrefs.GetFloat("BEST_WHEELIE_TIME");

                        if(totalWheelieTime > _bestWheelieTime)
                        {
                            PlayerPrefs.SetFloat("BEST_WHEELIE_TIME", wheelieTime);
                            PlayerPrefs.Save();
                        }

                        GameManager.Instance.AccumulateWheelieTime(wheelieTime);
                    }
                    else
                    {
                        //Debug.Log("Wheelie time too short. Wheelie not counted.");
                    }

                    wheelieStartTime = 0;
                }
            }
            else if (isWheelie && wheelieStartTime == 0)
            {
                isWheelie = false;
                wheelieGraceEndTime = 0;
            }

            if (!isWheelie && !hasJumped && hitBack.collider != null)
            {
                hasLanded = true;
            }
        }
    }

    public float CalculateMaxSpeed()
    {
        float wheelRadius = rearWheelCollider.radius;

        // Calculate the wheel's circumference (C = 2 * pi * r).
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        // Calculate the bike's maximum speed (Max speed = motor speed * wheel circumference).
        // This assumes no external forces (like drag, friction, etc.) and that the bike's motor can reach its maximum speed.
        float maxSpeed = Mathf.Abs(mo.motorSpeed) * wheelCircumference;

        return maxSpeed;
    }

    void CheckSpeedBoost()
    {
        if (isSpeedBoosted && Time.time > speedBoostEndTime)
        {
            isSpeedBoosted = false;
            motorSpeed = currentMotorSpeed;
        }
    }



    void HandleTrail()
    {
        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            if (rb.velocity.x > 0 && !bikeTrailRenderer.emitting)
            {
                StartCoroutine(FadeTrail(true));
            }
            else if (rb.velocity.x <= 0 && bikeTrailRenderer.emitting)
            {
                StartCoroutine(FadeTrail(false));
            }
        }
    }



    private IEnumerator FadeTrail(bool fadeIn)
    {
        float elapsedTime = 0f;
        float initialTime = bikeTrailRenderer.time;
        float targetTime = fadeIn ? defaultTrailTime : 0f;
        float duration = fadeIn ? 2f : 0.5f;

        if (fadeIn)
            bikeTrailRenderer.emitting = true;

        while (elapsedTime < duration)
        {
            float time = Mathf.Lerp(initialTime, targetTime, elapsedTime / duration);
            bikeTrailRenderer.time = time;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!fadeIn)
            bikeTrailRenderer.emitting = false;
    }



    public bool IsGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitBack.collider != null || hitFront.collider != null;
    }



    public bool IsRearWheelGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitBack.collider != null;
    }

    public bool IsFrontWheelGrounded()
    {
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitFront.collider != null;
    }



    private void Flip()
    {
        PlayerPrefs.SetInt("Faults", faults);
        PlayerPrefs.Save();
        internalFlipCount = 0;
        faults++;
        GameManager.Instance.UpdateFaultCountText();

        // If a previous flip is still in progress
        if (currentFlickerCoroutine != null)
            StopCoroutine(currentFlickerCoroutine);

        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, -Vector2.up, Mathf.Infinity, groundLayer);

        if (groundHit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, groundHit.point.y + 1f, transform.position.z);

            // Calculate the angle of the slope where the bike is landing
            float slopeAngle = Mathf.Atan2(groundHit.normal.y, groundHit.normal.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, slopeAngle - 90);
        }
        else
            transform.position += new Vector3(0, 1f, 0);

        currentFlickerCoroutine = StartCoroutine(RespawnCoroutine());
    }



    private IEnumerator RespawnCoroutine()
    {
        float flickerDuration = 0.5f;
        float disableColliderDuration = 0.2f;
        float startTime = Time.time;

        bikeBody.enabled = false;


        // Loop while the flicker duration hasn't passed
        while (Time.time - startTime < flickerDuration)
        {
            // Check if the disableColliderDuration has passed, if so re-enable the bike's collider
            if (!bikeBody.enabled && Time.time - startTime > disableColliderDuration)
                bikeBody.enabled = true;

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



    IEnumerator RotateBikeToFaceForward(float duration)
    {
        Vector3 startRotation = transform.eulerAngles;
        Vector3 endRotation = new Vector3(0, transform.eulerAngles.y, 0);

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = 1f - Mathf.Pow(1f - t, 3); // Ease out cubic function

            transform.eulerAngles = Vector3.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.eulerAngles = endRotation;
        rb.angularVelocity = 0f; // Stop any rotational movement
    }



    public float GetWheelieTime()
    {
        return totalWheelieTime;
    }



    public int GetFaultCount()
    {
        return faults;
    }



    private string FormatTime(float time)
    {
        int seconds = (int)time;
        int milliseconds = (int)((time - seconds) * 1000);

        return string.Format("{0:D2}.{1:D3}", seconds, milliseconds);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collided object is on the ground layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isBodyTouchingGround = true;

            // Determine the landing force based on the maximum height achieved
            float landingForce = BikeParticles.Instance.CalculateLandingForce(maxAirHeight, transform.position.y);

            // Play the landing particle effect
            BikeParticles.Instance.PlayLandingParticles(landingForce);

            // Reset the maximum height
            maxAirHeight = transform.position.y;
        }


    }

}