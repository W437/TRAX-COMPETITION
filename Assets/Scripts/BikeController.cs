using Cinemachine;
using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class BikeController : MonoBehaviour
{
    #region Variables
    public static BikeController Instance;

    public BikeParticles CurrentBikeParticles { get; private set; }
    public BikeComponents CurrentBikeComponents { get; private set; }

    //public MMFeedbacks StatsJuice;

    const string WHEELIE_DISTANCE = "WHEELIE_DISTANCE";
    const string BEST_WHEELIE_DISTANCE = "BEST_WHEELIE_DISTANCE";

    // External References
    public Bike bikeData;
    [SerializeField] private Bike[] bikes;

    TrailRenderer TrailRenderer;
    Rigidbody2D RB_Bike;
    WheelJoint2D BackWheel;
    WheelJoint2D FrontWheel;
    Rigidbody2D RB_BackWheel;
    Rigidbody2D RB_FrontWheel;
    CircleCollider2D RearWheelCollider;
    Transform BackWheelTransform;
    Transform FrontWheelTransform;
    CapsuleCollider2D BikeBody;
    SpriteRenderer BikeBodyRenderer;
    SpriteRenderer FrontWheelRenderer;
    SpriteRenderer BackWheelRenderer;
    ParticleSystem DirtParticles;
    ParticleSystem LandingParticles;
    Collider2D GroundCheckCollider;
    WheelJoint2D BikeWheelJoint;
    JointMotor2D BikeMotor;

    private GameObject currentTrailInstance;
    float motorSpeed;
    float maxTorque;
    float downwardForce;
    float accelerationTime;
    float groundCheckDistance;
    float initialMaxTorque; // Starting torque


    private Vector2 wheelieStartPosition;

    public float wheeliePoints;

    // Bike fields

    float maxAirRotationSpeed;
    float currentMotorSpeed = 0f;
    float initialMotorSpeed;
    float accelerationStartTime;
    bool isAccelerating = false;
    Vector2 prevPlayerVelocity;
    float prevMotorSpeed;
    float prevPlayerRotation;
    float prevAngularVelocity;

    Vector2 prevRearWheelVelocity;
    Vector2 prevFrontWheelVelocity;

    float prevRearWheelAngularVelocity;
    float prevFrontWheelAngularVelocity;


    // PlayerPrefs Save Data

    public float wheelieDistance = 0f;
    public int faults = 0;


    // Physics System

    float lastAirTime;

    // Double Mouse Press System
    [SerializeField] private float doublePressTime = 0.3f;

    int mouseClicks = 0;
    float mouseClickTimer = 0.0f;
    bool isDoubleMousePressed = false;
    Vector2 doubleClickForceDirection;
    Quaternion doubleClickRotation;
    bool isBeingPushedForward = false;
    float originalAngularDrag;
    Coroutine rotateBikeCoroutine = null;
    float flickStartTime;

    // Flip System
    public int flipCount = 0; // Flip counter
    [SerializeField] private float flipDelay = 0.5f; // time in seconds to wait before flipping the bike
    [SerializeField] private float flipTorque;
    float lastZRotation = 0f;
    float rotationCounter = 0;
    public int internalFlipCount = 0;
    bool hasLanded = false;
    Coroutine currentFlickerCoroutine = null;
    bool hasBeenUpsideDown = false;



    // Wheelie System
    float wheelieGracePeriod = 0f; // in seconds
    float wheelieGraceEndTime;
    bool isBodyTouchingGround = false;
    bool isWheelie = false;
    public float wheelieStartTime = 0f;
    public float totalWheelieTime;


    // Speed Boost System
    bool isSpeedBoosted = false;
    float speedBoostEndTime = 0f;
    float normalMotorSpeed;
    bool isBoosting = false; // Flag to track if the boost is active
    float boostMotorSpeed; // The target motor speed during the boost


    // Bike Trail System
    float defaultTrailTime = 0.2f;
    TrailManager trailManager;

    // Visuals
    Color originalBikeColor;
    Color originalFrontWheelColor;
    Color originalBackWheelColor;
    Color originalTrailColor;


    float maxAirHeight;
    // ----- VAR END ----- // 
    #endregion

    void Awake()
    {
        Instance = this;

    }

    void Start()
    {
        lastZRotation = transform.eulerAngles.z;
        // Bike Trail
    }

    void FixedUpdate()
    {
        if (isAccelerating && GameManager.Instance.gameState == GameState.Playing)
        {
            HandleBike();
        }
        else
        {
            if (BikeWheelJoint != null)
                BikeWheelJoint.useMotor = false;
            //else
                //Debug.Log("BikeWheelJoint is null");
        }
    }


    void Update()
    {
        if(GameManager.Instance.gameState == GameState.Playing)
        {
            //RB_Bike.isKinematic = false;
            maxAirHeight = Mathf.Max(maxAirHeight, transform.position.y);
            bool _isGrounded = IsGrounded();

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                mouseClicks++;
                if (mouseClicks == 1)
                    mouseClickTimer = Time.time;
            }

            if (mouseClicks > 0 && Time.time - mouseClickTimer > doublePressTime)
                mouseClicks = 0;

            if (mouseClicks >= 2)
            {
                mouseClicks = 0;
                if (!_isGrounded) // We only care if the bike is in the air
                {
                    // Save the direction that the bike is facing (assuming that the bike is facing its local up direction)
                    doubleClickForceDirection = Vector2.right;
                    doubleClickRotation = transform.rotation;
                    isDoubleMousePressed = true;
                    isBeingPushedForward = true;
                    originalAngularDrag = RB_Bike.angularDrag;
                    RB_Bike.angularDrag = 1f; // a high value to strongly resist rotation
                    RB_Bike.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
            }

            // Boost Forward

            if (isDoubleMousePressed)
            {
                RB_Bike.AddForce(doubleClickForceDirection * 2.5f, ForceMode2D.Impulse);
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
                    RB_Bike.angularDrag = originalAngularDrag; // reset angularDrag to its original value
                    RB_Bike.angularVelocity = 0f;
                    RB_Bike.constraints = RigidbodyConstraints2D.None;

                }
            }

            //-----------------------------------  Input

            if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                // Touch on screen or mouse click
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        accelerationStartTime = Time.time;
                        BikeWheelJoint.useMotor = true;
                    }

                    if (touch.phase == TouchPhase.Ended)
                    {
                        BikeWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }

                    // Call HandleBike for both moved and stationary phases
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        isAccelerating = true;
                    }
                }
                else // Mouse input
                {
                    if (Input.GetMouseButtonDown(0)) // Mouse click started
                    {
                        accelerationStartTime = Time.time;
                        BikeWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                    else if (Input.GetMouseButtonUp(0)) // Mouse click ended
                    {
                        BikeWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }
                    else if (Input.GetMouseButton(0)) // Mouse click continuing
                    {
                        isAccelerating = true;
                    }
                }
            }
            else
            {
                // No input, ensure isAccelerating is false
                isAccelerating = false;
            }


            if (Input.GetKeyUp(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }

            //--------------------

            CheckGroundContact();

            HandleFlips();

            if (IsRearWheelGrounded() && !IsFrontWheelGrounded())
            {
                if (!isWheelie)
                {
                    StartWheelie();
                }
            }
            else if (isWheelie && !IsRearWheelGrounded())
            {
                // Pause the wheelie if the bike jumps in the air
                PauseWheelie();
            }
            else if (isWheelie && IsFrontWheelGrounded())
            {
                EndWheelie();
            }

            CheckSpeedBoost();

            HandleTrail();
        }
    }


    public BikeComponents GetCurrentBikeComponents()
    {
        if (GameManager.Instance.GamePlayerBikeInstance != null)
        {
            return CurrentBikeComponents;
        }
        else 
        {
            Debug.LogWarning("No bike components were found.");
            return null; 
        }
    }


    public void LoadPlayerBike(int bikeId)
    {
        PlayerData data = SaveSystem.LoadPlayerData();
        if (!data.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("Bike not unlocked!");
            return;
        }

        // Destroy existing bike (if any)
        if (GameManager.Instance.GamePlayerBikeInstance != null)
        {
            Destroy(GameManager.Instance.GamePlayerBikeInstance);
        }

        // Find the BikeData with the matching bikeId in the BikeDataList
        Bike matchingBikeData = BikeController.Instance.bikes.FirstOrDefault(b => b.bikeId == bikeId);
        if (matchingBikeData == null)
        {
            Debug.Log("Bike not found in BikeDataList!");
            return;
        }

        // Instantiate the player bike
        GameManager.Instance.GamePlayerBikeInstance = Instantiate(matchingBikeData.bikePrefab, GameManager.Instance.playerObjectParent.transform);
        Debug.Log("Bike Instance: " + GameManager.Instance.GamePlayerBikeInstance.ToString());

        // Assign Particles
        CurrentBikeParticles = GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeParticles>();

        // Assign the current bike components
        // Get the BikeComponents
        CurrentBikeComponents = GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeComponents>();

        if (CurrentBikeComponents == null)
        {
            Debug.LogError("LoadPlayerBike: No BikeComponents script found on bike game object!");
            return;
        }
        else
        {
            // Access components
            RB_Bike = CurrentBikeComponents.RB_Bike;
            BackWheel = CurrentBikeComponents.BackWheel;
            FrontWheel = CurrentBikeComponents.FrontWheel;
            RB_BackWheel = CurrentBikeComponents.RB_BackWheel;
            RB_FrontWheel = CurrentBikeComponents.RB_FrontWheel;
            RearWheelCollider = CurrentBikeComponents.RearWheelCollider;
            BackWheelTransform = CurrentBikeComponents.BackWheelTransform;
            FrontWheelTransform = CurrentBikeComponents.FrontWheelTransform;
            BikeBody = CurrentBikeComponents.BikeBody;
            BikeBodyRenderer = CurrentBikeComponents.BikeBodyRenderer;
            FrontWheelRenderer = CurrentBikeComponents.FrontWheelRenderer;
            BackWheelRenderer = CurrentBikeComponents.BackWheelRenderer;
            DirtParticles = CurrentBikeComponents.DirtParticles;
            LandingParticles = CurrentBikeComponents.LandingParticles;
            GroundCheckCollider = CurrentBikeComponents.GroundCheckCollider;
            BikeWheelJoint = CurrentBikeComponents.BikeWheelJoint;
            BikeMotor = CurrentBikeComponents.BikeMotor;

            motorSpeed = CurrentBikeComponents.MotorSpeed;

            maxTorque = CurrentBikeComponents.MaxTorque;
            downwardForce = CurrentBikeComponents.DownwardForce;
            accelerationTime = CurrentBikeComponents.AccelerationTime;
            groundCheckDistance = CurrentBikeComponents.GroundCheckDistance;
            initialMaxTorque = CurrentBikeComponents.InitialMaxTorque;
            maxAirRotationSpeed = CurrentBikeComponents.MaxAirRotationSpeed;
            flipTorque = CurrentBikeComponents.FlipTorque;
            Debug.Log("All components linked.");
        }

        if (GameManager.Instance.firstLaunch)
        {
            GameManager.Instance.GamePlayerBikeInstance.SetActive(false);
            Debug.Log("First launch: " + GameManager.Instance.firstLaunch);
        }
        else
        {
            GameManager.Instance.GamePlayerBikeInstance.SetActive(true);
        }

        // Load the trail as a child of the bike
        int selectedTrailId = data.selectedTrailId;
        GameObject selectedTrail = TrailManager.Instance.GetTrailById(selectedTrailId).trailPrefab;

        if (selectedTrail != null)
        {
            // Instantiate the trail as a child of the bike
            currentTrailInstance = Instantiate(selectedTrail, GameManager.Instance.GamePlayerBikeInstance.transform);

            // Find the BikeTrail empty GameObject in the bike prefab
            Transform bikeTrailTransform = GameManager.Instance.GamePlayerBikeInstance.transform.Find("Bike Trail");
            if (bikeTrailTransform != null)
            {
                // Set the position of the trail to the position of the BikeTrail object
                currentTrailInstance.transform.position = bikeTrailTransform.position;
            }
            else
            {
                Debug.LogWarning("BikeTrail object not found in bike prefab. Trail position not set.");
            }
            // After instantiating the trail object
            TrailRenderer = currentTrailInstance.GetComponent<TrailRenderer>();

        }

        Debug.Log("Bike Loaded: " + GameManager.Instance.GamePlayerBikeInstance.ToString());

        // Set the game camera to follow the current bike instance
        CinemachineVirtualCamera virtualCamera = CameraController.Instance.gameCamera;
        virtualCamera.Follow = GameManager.Instance.GamePlayerBikeInstance.transform;
    }

    public void LoadPlayerTrail(int trailId)
    {
        PlayerData data = SaveSystem.LoadPlayerData();
        if (!data.unlockedTrails.Contains(trailId))
        {
            Debug.Log("Trail not unlocked!");
            return;
        }

        // Destroy existing trail (if any)
        if (GameManager.Instance.GamePlayerTrailInstance != null)
        {
            Destroy(GameManager.Instance.GamePlayerTrailInstance);
        }

        // Find the TrailData with the matching trailId in the TrailDataList
        Trail matchingTrailData = GameManager.Instance.TrailList.FirstOrDefault(t => t.trailId == trailId);
        if (matchingTrailData == null)
        {
            Debug.Log("Trail not found in TrailDataList!");
            return;
        }

        // Instantiate ---> Player Trail
        GameManager.Instance.GamePlayerTrailInstance = Instantiate(matchingTrailData.trailPrefab, GameManager.Instance.GamePlayerBikeInstance.transform);
        Debug.Log("Trail Instance: " + GameManager.Instance.GamePlayerTrailInstance.ToString());

        // You might need to do some additional setup for your trail here...
    }


    void HandleBike()
    {
        bool isGrounded = IsGrounded();
        if (isBeingPushedForward)
        {
            //rb.angularVelocity = 0; // Reset angular velocity
            // Don't apply torque while pushing forward
        }
        if (isGrounded && !isSpeedBoosted)
        {
            float elapsedTime = Time.time - accelerationStartTime;
            float progress = elapsedTime / accelerationTime;

            float easedProgress = 0.5f * (1 - Mathf.Cos(progress * Mathf.PI)); // Sine easing

            BikeMotor.maxMotorTorque = Mathf.Lerp(initialMaxTorque, maxTorque, easedProgress);
            BikeMotor.motorSpeed = motorSpeed;
            BikeWheelJoint.motor = BikeMotor;
        }

        else if (!isBeingPushedForward)
        {
            // Limit the flipping speed when in the air
            float currentRotationSpeed = RB_Bike.angularVelocity;

            if (Mathf.Abs(currentRotationSpeed) > maxAirRotationSpeed)
            {
                Debug.Log("Rotation Speed: " + currentRotationSpeed);
                RB_Bike.angularVelocity = Mathf.Sign(currentRotationSpeed) * maxAirRotationSpeed;
            }
            RB_Bike.AddTorque(flipTorque);
        }
    }


    public void PauseBike()
    {
        prevPlayerVelocity = RB_Bike.velocity;
        RB_Bike.velocity = Vector2.zero;
        prevPlayerRotation = RB_Bike.rotation;
        prevAngularVelocity = RB_Bike.angularVelocity;
        RB_Bike.angularVelocity = 0;
        prevMotorSpeed = BikeMotor.motorSpeed;
        BikeMotor.motorSpeed = 0;

        prevRearWheelVelocity = RB_BackWheel.velocity;

        prevFrontWheelVelocity = RB_FrontWheel.velocity;

        prevRearWheelAngularVelocity = RB_BackWheel.angularVelocity;
        prevFrontWheelAngularVelocity = RB_FrontWheel.angularVelocity;


        RB_Bike.isKinematic = true;
    }


    public void ResumeBike()
    {
        RB_Bike.isKinematic = false;

        RB_BackWheel.velocity = prevRearWheelVelocity;
        RB_FrontWheel.velocity = prevFrontWheelVelocity;

        RB_BackWheel.angularVelocity = prevRearWheelAngularVelocity;
        RB_FrontWheel.angularVelocity = prevFrontWheelAngularVelocity;


        RB_Bike.velocity = prevPlayerVelocity;
        RB_Bike.rotation = prevPlayerRotation;
        RB_Bike.angularVelocity = prevAngularVelocity;
        BikeMotor.motorSpeed = prevMotorSpeed;
    }


    void CheckGroundContact()
    {
        if (Physics2D.IsTouchingLayers(BikeBody, GameManager.Instance.groundLayer))
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
                //StatsJuice.PlayFeedbacks();
                Debug.Log("Intermediate Flip Count: " + internalFlipCount);
                hasBeenUpsideDown = false; // Reset for the next flip
            }
        }

        lastZRotation = transform.eulerAngles.z;
    }


    void StartWheelie()
    {
        isWheelie = true;
        wheelieStartPosition = BackWheelTransform.position;
        Invoke(nameof(BeginWheelie), wheelieGracePeriod);
    }


    void BeginWheelie()
    {
        if (isWheelie) // In case the wheelie got cancelled before the grace period
        {
            wheelieStartTime = Time.time;
        }
    }


    void PauseWheelie()
    {
        if (wheelieStartTime != 0)
        {
            float wheelieTime = Time.time - wheelieStartTime;
            PlayerPrefs.SetFloat("WHEELIE_TIME", wheelieTime);
            wheelieStartTime = 0;
        }
    }
    
    
    void EndWheelie()
    {
        if (isWheelie)
        {
            isWheelie = false;
            if (wheelieStartTime != 0) // If wheelieStartTime is not set, that means the wheelie was paused due to jumping
            {
                float wheelieTime = Time.time - wheelieStartTime;
                float wheelieDistance = Vector2.Distance(wheelieStartPosition, BackWheelTransform.position);
                wheeliePoints = wheelieTime * wheelieDistance;

                PlayerPrefs.SetFloat(WHEELIE_DISTANCE, wheeliePoints);
                PlayerPrefs.Save();

                float _bestWheelieDistance = PlayerPrefs.GetFloat(BEST_WHEELIE_DISTANCE);

                if (wheeliePoints > _bestWheelieDistance)
                {
                    PlayerPrefs.SetFloat(BEST_WHEELIE_DISTANCE, wheeliePoints);
                    PlayerPrefs.Save();
                }

                GameManager.Instance.AccumulateWheelieTime(wheeliePoints); // Adjust the AccumulateWheelieTime function accordingly

                wheelieStartTime = 0;
            }

            RaycastHit2D hitBack = Physics2D.Raycast(BackWheelTransform.position, -Vector2.up, groundCheckDistance, GameManager.Instance.groundLayer);
            RaycastHit2D hitFront = Physics2D.Raycast(FrontWheelTransform.position, -Vector2.up, groundCheckDistance, GameManager.Instance.groundLayer);

            bool hasJumped = (hitFront.collider == null && !IsRearWheelGrounded());

            if (!isWheelie && !hasJumped && hitBack.collider != null)
            {
                hasLanded = true;
            }
        }
    }


    public float CalculateMaxSpeed()
    {
        float wheelRadius = RearWheelCollider.radius;

        // Calculate the wheel's circumference (C = 2 * pi * r).
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        // Calculate the bike's maximum speed (Max speed = motor speed * wheel circumference).
        // This assumes no external forces (like drag, friction, etc.) and that the bike's motor can reach its maximum speed.
        float maxSpeed = Mathf.Abs(BikeMotor.motorSpeed) * wheelCircumference;

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

    private bool wasPreviouslyGrounded = true;
    private Coroutine trailFadeCoroutine = null;

    void HandleTrail()
    {
        bool isMovingForward = RB_Bike.velocity.x > 0;
        bool isGrounded = IsGrounded();

        // Start fading in if moving forward and not currently fading in or out
        if (isMovingForward && trailFadeCoroutine == null && !TrailRenderer.emitting)
        {
            trailFadeCoroutine = StartCoroutine(FadeTrail(true, 0.5f));
        }
        // Start fading out if not moving forward or not grounded, and not currently fading in or out
        else if ((!isMovingForward || !isGrounded) && trailFadeCoroutine == null && TrailRenderer.emitting)
        {
            trailFadeCoroutine = StartCoroutine(FadeTrail(false, 1.5f));
        }
    }


    private IEnumerator FadeTrail(bool fadeIn, float duration)
    {
        float elapsedTime = 0f;
        float initialTime = TrailRenderer.time;
        float targetTime = fadeIn ? defaultTrailTime : 0f;

        if (fadeIn)
        {
            TrailRenderer.emitting = true;
        }

        while (elapsedTime < duration)
        {
            float time = Mathf.Lerp(initialTime, targetTime, elapsedTime / duration);
            TrailRenderer.time = time;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Only stop emitting when fading out
        if (!fadeIn)
        {
            TrailRenderer.emitting = false;
        }

        trailFadeCoroutine = null; // Reset the coroutine reference
    }


    public float GetBikeSpeed()
    {
        return RB_Bike.velocity.magnitude;
    }


    public bool IsGrounded()
    {
        //RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //return hitBack.collider != null || hitFront.collider != null;
        return GroundCheckCollider.IsTouchingLayers(GameManager.Instance.groundLayer);
    }


    public bool IsRearWheelGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(BackWheelTransform.position, -Vector2.up, groundCheckDistance, GameManager.Instance.groundLayer);
        return hitBack.collider != null;
    }


    public bool IsFrontWheelGrounded()
    {
        RaycastHit2D hitFront = Physics2D.Raycast(FrontWheelTransform.position, -Vector2.up, groundCheckDistance, GameManager.Instance.groundLayer);
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

        var _playerBike = GameManager.Instance.GamePlayerBikeInstance.gameObject.transform;
        RaycastHit2D groundHit = Physics2D.Raycast(_playerBike.position, -Vector2.up, Mathf.Infinity, GameManager.Instance.groundLayer);

        if (groundHit.collider != null)
        {
            _playerBike.position = new Vector3(_playerBike.position.x, groundHit.point.y + 1f, _playerBike.position.z);

            // Calculate the angle of the slope where the bike is landing
            float slopeAngle = Mathf.Atan2(groundHit.normal.y, groundHit.normal.x) * Mathf.Rad2Deg;
            _playerBike.rotation = Quaternion.Euler(0, 0, slopeAngle - 90);
        }
        else
            _playerBike.position += new Vector3(0, 1f, 0);

        currentFlickerCoroutine = StartCoroutine(RespawnCoroutine());
    }


    public float GetVerticalVelocity()
    {
        return RB_Bike.velocity.y;
    }


    private IEnumerator RespawnCoroutine()
    {
        float flickerDuration = 0.5f;
        float disableColliderDuration = 0.2f;
        float startTime = Time.time;

        BikeBody.enabled = false;


        // Loop while the flicker duration hasn't passed
        while (Time.time - startTime < flickerDuration)
        {
            // Check if the disableColliderDuration has passed, if so re-enable the bike's collider
            if (!BikeBody.enabled && Time.time - startTime > disableColliderDuration)
                BikeBody.enabled = true;

            // Make the bike and its components transparent
            //GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeComponents>().BikeColorAlpha = 0.5f;
            //TrailManager.Instance.ChangeTrailAlpha(currentTrailInstance, 0.5f);
            yield return new WaitForSeconds(0.1f);

            // Return to the original colors
            // BikeBodyRenderer.color = new Color(originalBikeColor.r, originalBikeColor.g, originalBikeColor.b, 1f);
            // FrontWheelRenderer.color = new Color(originalFrontWheelColor.r, originalFrontWheelColor.g, originalFrontWheelColor.b, 1f);
            // BackWheelRenderer.color = new Color(originalBackWheelColor.r, originalBackWheelColor.g, originalBackWheelColor.b, 1f);
            // TrailRenderer.startColor = new Color(originalTrailColor.r, originalTrailColor.g, originalTrailColor.b, 1f);
            // GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeComponents>().BikeColorAlpha = 1f;
            // TrailManager.Instance.ChangeTrailAlpha(currentTrailInstance, 1f);
            yield return new WaitForSeconds(0.1f);
        }

        // After the loop, ensure the bike's collider is re-enabled and the color is set back to its original state
        BikeBody.enabled = true;
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
        Vector2 boostPosition = BackWheelTransform.position; // Position to apply the boost force

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float currentMotorSpeed = Mathf.Lerp(motorSpeed, boostMotorSpeed, progress);

            // Calculate the direction of the boost
            Vector2 boostDirection = transform.right; // assuming the front of the bike is its right side

            // Apply the boost force to the bike's rigidbody at the boost position
            RB_Bike.AddForceAtPosition(boostDirection * amount * RB_Bike.mass, boostPosition, ForceMode2D.Force);

            // Counteract the leaning effect by applying an opposite force
            Vector2 currentUpVector = transform.up;
            Vector2 leanForce = -Vector2.Dot(currentUpVector, initialUpVector) * transform.forward * amount * RB_Bike.mass;
            RB_Bike.AddForceAtPosition(leanForce, boostPosition, ForceMode2D.Force);

            // Apply limited rotation force during the boost
            float rotationForceMultiplier = isSpeedBoosted ? 0.3f : 1f;
            RB_Bike.AddTorque(-flipTorque * rotationForceMultiplier);
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
        RB_Bike.angularVelocity = 0f; // Stop any rotational movement
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

    public void MoveAtConstantSpeed()
    {
        RB_Bike.AddForce(new Vector2(105f, 0), ForceMode2D.Force);
    }

    public float CalculateLandingForce(float maxAirHeight, float currentHeight)
    {
        return maxAirHeight - currentHeight;
    }

    public Bike[] GetAllBikes()
    {
        return bikes;
    }

    public Bike GetBikeById(int id)
    {
        return bikes.FirstOrDefault(b => b.bikeId == id);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collided object is on the ground layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isBodyTouchingGround = true;

            // Determine the landing force based on the maximum height achieved
            float landingForce = CalculateLandingForce(maxAirHeight, transform.position.y);

            // Play the landing particle effect
            CurrentBikeParticles.PlayLandingParticles(landingForce);

            // Reset the maximum height
            maxAirHeight = transform.position.y;
        }
    }

}