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
    public static BikeController Instance;

    public BikeParticles CurrentBikeParticles { get; private set; }
    public BikeComponents CurrentBikeComponents { get; private set; }

    //public MMFeedbacks StatsJuice;

    #region Bike Components & Config
    const string WHEELIE_DISTANCE = "WHEELIE_DISTANCE";
    const string BEST_WHEELIE_DISTANCE = "BEST_WHEELIE_DISTANCE";


    [SerializeField] private Bike[] bikeList;
    public Bike bikeData;
    private GameObject currentTrailInstance;

    TrailRenderer trailRenderer;
    Rigidbody2D bikeRb;
    WheelJoint2D rearWheel,frontWheel;
    Rigidbody2D rearWheelRb, frontWheelRb;
    CircleCollider2D rearWheelCol;
    Transform backWheelTransform, frontWheelTransform;
    CapsuleCollider2D bikeBodyCol;
    SpriteRenderer bikeBodyRenderer;
    SpriteRenderer frontWheelSpriteRenderer, backWheelSpriteRenderer;
    ParticleSystem bikeDirtParticles, bikeLandingParticles;
    Collider2D bikeGroundCheckCol;
    WheelJoint2D bikeRearWheelJoint;
    JointMotor2D bikeMotor;
    Vector2 wheelieStartPosition;

    float bikeMotorSpeed;
    float bikeMaxTorque;
    float bikeDownwardForce;
    float bikeAccelerationTime;
    float bikeGroundCheckDistance;
    float bikeInitialMaxTorque; // Starting torque
    public float wheeliePoints;
    float maxAirRotationSpeed;
    float currentMotorSpeed = 0f;
    float initialMotorSpeed;
    float accelerationStartTime;
    bool isAccelerating = false;
    float prevMotorSpeed;
    float prevPlayerRotation;
    float prevAngularVelocity;
    Vector2 prevPlayerVelocity;
    Vector2 prevRearWheelVelocity, prevFrontWheelVelocity;

    float prevRearWheelAngularVelocity;
    float prevFrontWheelAngularVelocity;
    float wheelieDistance = 0f;
    public int faults = 0;
    float lastAirTime;
    float maxAirHeight;

    [SerializeField] float doublePressTime = 0.3f; // Double Mouse Press System
    int mouseClicks = 0;
    float mouseClickTimer = 0.0f;
    float originalAngularDrag;
    float bikeBoostTime;
    Vector2 doubleClickForceDirection;
    Quaternion doubleClickRotation;
    Coroutine rotateBikeCoroutine = null;
    bool isBeingPushedForward = false;
    bool isDoubleMousePressed = false;

    // Flip System
    public int flipCount = 0;
    [SerializeField] float flipDelay = 0.5f; // time in seconds to wait before flipping the bike
    [SerializeField] float flipTorque;
    float lastZRotation = 0f;
    float rotationCounter = 0;
    int internalFlipCount = 0;
    bool hasLanded = false;
    Coroutine currentFlickerCoroutine = null;
    bool hasBeenUpsideDown = false;


    // Wheelie System
    float wheelieGracePeriod = 0.1f; // in seconds
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

    #endregion

    // Visuals
    Color originalBikeColor;
    Color originalFrontWheelColor;
    Color originalBackWheelColor;
    Color originalTrailColor;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(bikeRb != null)
            lastZRotation = bikeRb.transform.eulerAngles.z;
    }

    void FixedUpdate()
    {
        if (isAccelerating && GameManager.Instance.gameState == GameState.Playing)
            HandleBike();
        else if (bikeRearWheelJoint != null)
            bikeRearWheelJoint.useMotor = false;    
    }


    void Update()
    {
        var _gameState = GameManager.Instance.gameState;
        if(_gameState == GameState.Playing)
        {
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
                if (!_isGrounded)
                {
                    // Save the direction that the bike is facing
                    doubleClickForceDirection = Vector2.right;
                    doubleClickRotation = transform.rotation;
                    isDoubleMousePressed = true;
                    isBeingPushedForward = true;
                    originalAngularDrag = bikeRb.angularDrag;
                    bikeRb.angularDrag = 1f; // resist rotation
                    //bikeRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
            }

            if (isDoubleMousePressed)
            {
                bikeRb.AddForce(doubleClickForceDirection * 2.5f, ForceMode2D.Impulse);
                isDoubleMousePressed = false;
                isBeingPushedForward = true;
                bikeBoostTime = Time.time; // Set the flick start time

                if (rotateBikeCoroutine != null)
                {
                    StopCoroutine(rotateBikeCoroutine);
                }
                rotateBikeCoroutine = StartCoroutine(RotateBikeToFaceForward(0.5f));
            }

            else if (isBeingPushedForward)
            {
                if (Time.time >= bikeBoostTime + 0.5f)
                {
                    isBeingPushedForward = false; // Stop pushing forward
                    bikeRb.angularDrag = originalAngularDrag; // reset angularDrag to its original value
                    bikeRb.angularVelocity = 0f;
                    bikeRb.constraints = RigidbodyConstraints2D.None;

                }
            }

            if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                if (Input.touchCount > 0)
                {
                    Touch _touch = Input.GetTouch(0);

                    if (_touch.phase == TouchPhase.Began)
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                    }

                    if (_touch.phase == TouchPhase.Ended)
                    {
                        bikeRearWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }

                    if (_touch.phase == TouchPhase.Moved || _touch.phase == TouchPhase.Stationary)
                        isAccelerating = true;
                }
                else
                {
                    if (Input.GetMouseButtonDown(0)) // Mouse click started
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                    else if (Input.GetMouseButtonUp(0)) // Mouse click ended
                    {
                        bikeRearWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }
                    else if (Input.GetMouseButton(0)) // Mouse click continuing -- need?
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                }
            }
            else
                // No input
                isAccelerating = false;


            if (Input.GetKeyUp(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }

            //Debug.Log("HasLanded: " + hasLanded);
            CheckGroundContact();
            CheckSpeedBoost();
            HandleTrail();
            HandleFlips();
            // Wheelie check
            if (!isWheelie && IsRearWheelGrounded() && !IsFrontWheelGrounded())
                StartWheelie();
            else if (isWheelie && !IsGrounded())
                PauseWheelie();
            else if (isWheelie && IsFrontWheelGrounded())
                EndWheelie();
        }
    }


    public BikeComponents GetCurrentBikeComponents()
    {
        if (GameManager.Instance.GamePlayerBikeInstance != null)
            return CurrentBikeComponents;
        else 
            Debug.LogWarning("No bike components were found.");
            return null; 
    }


    public void LoadPlayerBike(int bikeId)
    {
        PlayerData _playerData = SaveSystem.LoadPlayerData();
        if (!_playerData.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("Bike not unlocked!");
            return;
        }

        // Destroy existing bike
        if (GameManager.Instance.GamePlayerBikeInstance != null)
            Destroy(GameManager.Instance.GamePlayerBikeInstance);

        // Find the bike with the matching bikeId
        Bike _matchingBikeData = bikeList.FirstOrDefault(b => b.bikeId == bikeId);
        if (_matchingBikeData == null)
        {
            Debug.Log("Bike not found in Bike list!");
            return;
        }
        
        // Instantiate the player bike
        GameManager.Instance.GamePlayerBikeInstance = Instantiate(_matchingBikeData.bikePrefab, GameManager.Instance.playerObjectParent.transform);
        Debug.Log("Bike Instance: " + GameManager.Instance.GamePlayerBikeInstance.ToString());
        // Assign
        CurrentBikeParticles = GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeParticles>();
        CurrentBikeComponents = GameManager.Instance.GamePlayerBikeInstance.GetComponent<BikeComponents>();

        if (CurrentBikeComponents == null)
        {
            Debug.LogError("LoadPlayerBike: No BikeComponents script found on bike game object!");
            return;
        }
        else
        {
            bikeRb = CurrentBikeComponents.RB_Bike;
            rearWheel = CurrentBikeComponents.BackWheel;
            frontWheel = CurrentBikeComponents.FrontWheel;
            rearWheelRb = CurrentBikeComponents.RB_BackWheel;
            frontWheelRb = CurrentBikeComponents.RB_FrontWheel;
            rearWheelCol = CurrentBikeComponents.RearWheelCollider;
            backWheelTransform = CurrentBikeComponents.BackWheelTransform;
            frontWheelTransform = CurrentBikeComponents.FrontWheelTransform;
            bikeBodyCol = CurrentBikeComponents.BikeBody;
            bikeBodyRenderer = CurrentBikeComponents.BikeBodyRenderer;
            frontWheelSpriteRenderer = CurrentBikeComponents.FrontWheelRenderer;
            backWheelSpriteRenderer = CurrentBikeComponents.BackWheelRenderer;
            bikeDirtParticles = CurrentBikeComponents.DirtParticles;
            bikeLandingParticles = CurrentBikeComponents.LandingParticles;
            bikeGroundCheckCol = CurrentBikeComponents.GroundCheckCollider;
            bikeRearWheelJoint = CurrentBikeComponents.BikeWheelJoint;
            bikeMotor = CurrentBikeComponents.BikeMotor;
            bikeMotorSpeed = CurrentBikeComponents.MotorSpeed;
            bikeMaxTorque = CurrentBikeComponents.MaxTorque;
            bikeDownwardForce = CurrentBikeComponents.DownwardForce;
            bikeAccelerationTime = CurrentBikeComponents.AccelerationTime;
            bikeGroundCheckDistance = CurrentBikeComponents.GroundCheckDistance;
            bikeInitialMaxTorque = CurrentBikeComponents.InitialMaxTorque;
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
        int _selectedTrailId = _playerData.selectedTrailId;
        GameObject selectedTrail = TrailManager.Instance.GetTrailById(_selectedTrailId).trailPrefab;

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
            trailRenderer = currentTrailInstance.GetComponent<TrailRenderer>();

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
            float progress = elapsedTime / bikeAccelerationTime;

            float easedProgress = 0.5f * (1 - Mathf.Cos(progress * Mathf.PI)); // Sine easing

            bikeMotor.maxMotorTorque = Mathf.Lerp(bikeInitialMaxTorque, bikeMaxTorque, easedProgress);
            bikeMotor.motorSpeed = bikeMotorSpeed;
            bikeRearWheelJoint.motor = bikeMotor;
        }

        else if (!isBeingPushedForward)
        {
            // Limit the flipping speed when in the air
            float currentRotationSpeed = bikeRb.angularVelocity;

            if (Mathf.Abs(currentRotationSpeed) > maxAirRotationSpeed)
            {
                //Debug.Log("Rotation Speed: " + currentRotationSpeed);
                bikeRb.angularVelocity = Mathf.Sign(currentRotationSpeed) * maxAirRotationSpeed;
            }
            bikeRb.AddTorque(flipTorque);
        }
    }


    public void PauseBike()
    {
        prevPlayerVelocity = bikeRb.velocity;
        bikeRb.velocity = Vector2.zero;
        prevPlayerRotation = bikeRb.rotation;
        prevAngularVelocity = bikeRb.angularVelocity;
        bikeRb.angularVelocity = 0;
        prevMotorSpeed = bikeMotor.motorSpeed;
        bikeMotor.motorSpeed = 0;

        prevRearWheelVelocity = rearWheelRb.velocity;

        prevFrontWheelVelocity = frontWheelRb.velocity;

        prevRearWheelAngularVelocity = rearWheelRb.angularVelocity;
        prevFrontWheelAngularVelocity = frontWheelRb.angularVelocity;


        bikeRb.isKinematic = true;
    }


    public void ResumeBike()
    {
        bikeRb.isKinematic = false;

        rearWheelRb.velocity = prevRearWheelVelocity;
        frontWheelRb.velocity = prevFrontWheelVelocity;

        rearWheelRb.angularVelocity = prevRearWheelAngularVelocity;
        frontWheelRb.angularVelocity = prevFrontWheelAngularVelocity;


        bikeRb.velocity = prevPlayerVelocity;
        bikeRb.rotation = prevPlayerRotation;
        bikeRb.angularVelocity = prevAngularVelocity;
        bikeMotor.motorSpeed = prevMotorSpeed;
    }


    void CheckGroundContact()
    {
        if (Physics2D.IsTouchingLayers(bikeBodyCol, GameManager.Instance.groundLayer))
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
        else if (!IsFrontWheelGrounded())
        {
            float rotationDiff = bikeRb.transform.eulerAngles.z - lastZRotation;
            if (rotationDiff > 180f) rotationDiff -= 360f;
            else if (rotationDiff < -180f) rotationDiff += 360f;

            rotationCounter += rotationDiff;

            // Check if the bike has been upside down
            if (bikeRb.transform.eulerAngles.z > 90 && bikeRb.transform.eulerAngles.z < 270)
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

        
        bool hasJumped = !IsGrounded();

        if (!isWheelie && !hasJumped && IsRearWheelGrounded() && IsFrontWheelGrounded())
        {
            hasLanded = true;

        }

        lastZRotation = bikeRb.transform.eulerAngles.z;
    }


    void StartWheelie()
    {
        isWheelie = true;
        wheelieStartPosition = backWheelTransform.position;
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
                float wheelieDistance = Vector2.Distance(wheelieStartPosition, backWheelTransform.position);
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

            RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.groundLayer);
            RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.groundLayer);
        }
    }


    public float CalculateMaxSpeed()
    {
        float wheelRadius = rearWheelCol.radius;

        // Calculate the wheel's circumference (C = 2 * pi * r).
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        // Calculate the bike's maximum speed (Max speed = motor speed * wheel circumference).
        // This assumes no external forces (like drag, friction, etc.) and that the bike's motor can reach its maximum speed.
        float maxSpeed = Mathf.Abs(bikeMotor.motorSpeed) * wheelCircumference;

        return maxSpeed;
    }


    void CheckSpeedBoost()
    {
        if (isSpeedBoosted && Time.time > speedBoostEndTime)
        {
            isSpeedBoosted = false;
            bikeMotorSpeed = currentMotorSpeed;
        }
    }

    private bool wasPreviouslyGrounded = true;
    private Coroutine trailFadeCoroutine = null;

    void HandleTrail()
    {
        bool isMovingForward = bikeRb.velocity.x > 0;
        bool isGrounded = IsGrounded();

        // Start fading in if moving forward and not currently fading in or out
        if (isMovingForward && trailFadeCoroutine == null && !trailRenderer.emitting)
        {
            trailFadeCoroutine = StartCoroutine(FadeTrail(true, 0.5f));
        }
        // Start fading out if not moving forward or not grounded, and not currently fading in or out
        else if ((!isMovingForward || !isGrounded) && trailFadeCoroutine == null && trailRenderer.emitting)
        {
            trailFadeCoroutine = StartCoroutine(FadeTrail(false, 1.5f));
        }
    }


    private IEnumerator FadeTrail(bool fadeIn, float duration)
    {
        float elapsedTime = 0f;
        float initialTime = trailRenderer.time;
        float targetTime = fadeIn ? defaultTrailTime : 0f;

        if (fadeIn)
        {
            trailRenderer.emitting = true;
        }

        while (elapsedTime < duration)
        {
            float time = Mathf.Lerp(initialTime, targetTime, elapsedTime / duration);
            trailRenderer.time = time;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Only stop emitting when fading out
        if (!fadeIn)
        {
            trailRenderer.emitting = false;
        }

        trailFadeCoroutine = null; // Reset the coroutine reference
    }


    public float GetBikeSpeed()
    {
        return bikeRb.velocity.magnitude;
    }


    public bool IsGrounded()
    {
        //RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //return hitBack.collider != null || hitFront.collider != null;
        return bikeGroundCheckCol.IsTouchingLayers(GameManager.Instance.groundLayer);
    }


    public bool IsRearWheelGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.groundLayer);
        return hitBack.collider != null;
    }


    public bool IsFrontWheelGrounded()
    {
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.groundLayer);
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
        return bikeRb.velocity.y;
    }


    private IEnumerator RespawnCoroutine()
    {
        float flickerDuration = 0.5f;
        float disableColliderDuration = 0.2f;
        float startTime = Time.time;

        bikeBodyCol.enabled = false;


        // Loop while the flicker duration hasn't passed
        while (Time.time - startTime < flickerDuration)
        {
            // Check if the disableColliderDuration has passed, if so re-enable the bike's collider
            if (!bikeBodyCol.enabled && Time.time - startTime > disableColliderDuration)
                bikeBodyCol.enabled = true;

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
        bikeBodyCol.enabled = true;
    }


    public void ApplySpeedBoost(SpeedBoost.SpeedBoostData data)
    {
        if (!isBoosting)
        {
            isBoosting = true;
            boostMotorSpeed = bikeMotorSpeed + data.Amount;
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
            float currentMotorSpeed = Mathf.Lerp(bikeMotorSpeed, boostMotorSpeed, progress);

            // Calculate the direction of the boost
            Vector2 boostDirection = transform.right; // assuming the front of the bike is its right side

            // Apply the boost force to the bike's rigidbody at the boost position
            bikeRb.AddForceAtPosition(boostDirection * amount * bikeRb.mass, boostPosition, ForceMode2D.Force);

            // Counteract the leaning effect by applying an opposite force
            Vector2 currentUpVector = transform.up;
            Vector2 leanForce = -Vector2.Dot(currentUpVector, initialUpVector) * transform.forward * amount * bikeRb.mass;
            bikeRb.AddForceAtPosition(leanForce, boostPosition, ForceMode2D.Force);

            // Apply limited rotation force during the boost
            float rotationForceMultiplier = isSpeedBoosted ? 0.3f : 1f;
            bikeRb.AddTorque(-flipTorque * rotationForceMultiplier);
            // HEY SOZO!
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBoosting = false;
        bikeMotorSpeed = boostMotorSpeed; // Set the motor speed to the boosted speed
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
        bikeRb.angularVelocity = 0f; // Stop any rotational movement
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
        bikeRb.AddForce(new Vector2(105f, 0), ForceMode2D.Force);
    }

    public float CalculateLandingForce(float maxAirHeight, float currentHeight)
    {
        return maxAirHeight - currentHeight;
    }

    public Bike[] GetAllBikes()
    {
        return bikeList;
    }

    public Bike GetBikeById(int id)
    {
        return bikeList.FirstOrDefault(b => b.bikeId == id);
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