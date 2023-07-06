using Cinemachine;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static GameManager;
using Lofelt.NiceVibrations;

public class BikeController : MonoBehaviour
{
    public static BikeController Instance;
    private TrailManager TrailManager;
    public bool CAN_CONTROL = false; // debugging

    /////////////////////////////////////////////////////////////////////////////////////
    /////// BIKE STUFF
    /////////////////////////////////////////////////////////////////////////////////////

    public BikeParticles CurrentBikeParticles { get; private set; }
    public BikeComponents CurrentBikeComponents { get; private set; }

    // Distance counter system
    private float previousXPosition;
    private float totalDistance;
    private int frameCounter = 0;
    private int frameThreshold = 10; // count every 10 frames for efficiency.
    public IEnumerator saveDistanceCoroutine;

    [SerializeField] private Bike[] BikeList;
    [NonSerialized] public Bike bikeData;
    private GameObject currentTrailInstance;

    private TrailRenderer trailRenderer;
    private Rigidbody2D bikeRb;
    private WheelJoint2D rearWheel,frontWheel;
    private Rigidbody2D rearWheelRb, frontWheelRb;
    private CircleCollider2D rearWheelCol;
    private Transform backWheelTransform, frontWheelTransform;
    private CapsuleCollider2D bikeBodyCol;
    private SpriteRenderer bikeBodyRenderer;
    private SpriteRenderer frontWheelSpriteRenderer, backWheelSpriteRenderer;
    private ParticleSystem bikeDirtParticles, bikeLandingParticles;
    private Collider2D bikeGroundCheckCol;
    [NonSerialized] public WheelJoint2D bikeRearWheelJoint;
    private JointMotor2D bikeMotor;
    private Vector2 wheelieStartPosition;
    private Coroutine trailFadeCoroutine = null;


    private float _previousSpeed = 0;
    private float _acceleration = 0;
    private float bikeMotorSpeed;
    private float bikeMaxTorque;
    private float bikeDownwardForce;
    private float bikeAccelerationTime;
    private float bikeGroundCheckDistance;
    private float bikeInitialMaxTorque; // Starting torque
    public float wheeliePoints;
    private float maxAirRotationSpeed;
    private float currentMotorSpeed = 0f;
    private float initialMotorSpeed;
    private float accelerationStartTime;
    public bool isAccelerating = false; // private set

    private float prevMotorSpeed;
    private float prevPlayerRotation;
    private float prevAngularVelocity;
    private Vector2 prevPlayerVelocity;
    private Vector2 prevRearWheelVelocity, prevFrontWheelVelocity;

    private float prevRearWheelAngularVelocity;
    private float prevFrontWheelAngularVelocity;
    public int faults = 0;
    private float lastAirTime;
    private float maxAirHeight;

    [SerializeField] private float doublePressTime = 0.3f; // Double Mouse Press System
    private int mouseClicks = 0;
    private float mouseClickTimer = 0.0f;
    private float originalAngularDrag;
    private float bikeBoostTime;
    private Vector2 doubleClickForceDirection;
    private Quaternion doubleClickRotation;
    private Coroutine rotateBikeCoroutine = null;
    private bool isBeingPushedForward = false;
    private bool isDoubleMousePressed = false;

    // Flip System
    public int flipCount = 0;
    [SerializeField] private float flipDelay = 0.5f; // time in seconds to wait before fault flipping the bike
    [SerializeField] private float flipTorque;
    private float lastZRotation = 0f;
    private float rotationCounter = 0;
    private int internalFlipCount = 0;
    private bool hasLanded = false;
    private Coroutine currentFlickerCoroutine = null;
    private bool hasBeenUpsideDown = false;


    // Wheelie System
    private float wheelieGracePeriod = 0.1f; // in seconds
    private float wheelieGraceEndTime;

    private bool isWheelie = false;
    public float wheelieStartTime = 0f;
    public float totalWheelieTime;


    // Speed Boost System
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;
    private float normalMotorSpeed;
    private bool isBoosting = false; 
    private float boostMotorSpeed; // The target motor speed during the boost


    // Bike Trail System
    float defaultTrailTime = 0.2f;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(bikeRb != null)
        {
            previousXPosition = GameManager.Instance.InGAME_PlayerBike.transform.position.x;
            lastZRotation = bikeRb.transform.eulerAngles.z;
/*            saveDistanceCoroutine = SaveDistanceEveryFewSeconds(30.0f);
            StartCoroutine(saveDistanceCoroutine);*/
        }

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
        // For Haptic System
        float currentSpeed = GetBikeSpeed();
        _acceleration = (currentSpeed - _previousSpeed) / Time.deltaTime;
        _previousSpeed = currentSpeed;

        var _gameState = GameManager.Instance.gameState;
        if(_gameState == GameState.Playing)
        {
            maxAirHeight = Mathf.Max(maxAirHeight, transform.position.y);
            bool _isGrounded = IsGrounded();

            // Distance tracker for savedata
            // Save distance every 30s for efficiency
            frameCounter++;
            if (frameCounter >= frameThreshold)
            {
                float distanceThisFrame = Mathf.Abs(GameManager.Instance.InGAME_PlayerBike.transform.position.x - previousXPosition);
                totalDistance += distanceThisFrame;
                previousXPosition = GameManager.Instance.InGAME_PlayerBike.transform.position.x;
                frameCounter = 0;
            }

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
                if (Time.time >= bikeBoostTime + 0.05f)
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

                    if (_touch.phase == TouchPhase.Began && !isAccelerating)
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }

                    if (_touch.phase == TouchPhase.Ended)
                    {
                        bikeRearWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }

                    if ((_touch.phase == TouchPhase.Moved || _touch.phase == TouchPhase.Stationary) && !isAccelerating)
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !isAccelerating) 
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        bikeRearWheelJoint.useMotor = false;
                        isAccelerating = false;
                    }
                    else if (Input.GetMouseButton(0) && !isAccelerating)
                    {
                        accelerationStartTime = Time.time;
                        bikeRearWheelJoint.useMotor = true;
                        isAccelerating = true;
                    }
                }

            }
            else
                isAccelerating = false;

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



    public float GetBikeAcceleration()
    {
        return _acceleration;
    }

    public float GetTotalDistanceInKilometers()
    {
        return totalDistance / 1000f;
    }

    public void StopSavingDistance()
    {
        if (saveDistanceCoroutine != null)
        {
            StopCoroutine(saveDistanceCoroutine);
            saveDistanceCoroutine = null;
        }
    }

    public void LoadPlayerBike(int bikeId)
    {
        PlayerData _playerData = SaveSystem.LoadPlayerData();
        if (!_playerData.UNLOCKED_BIKES.Contains(bikeId))
        {
            Debug.Log("Bike not unlocked!");
            return;
        }

        // Destroy existing bike
        if (GameManager.Instance.InGAME_PlayerBike != null)
            Destroy(GameManager.Instance.InGAME_PlayerBike);

        // Find the bike with the matching bikeId
        Bike _matchingBikeData = BikeList.FirstOrDefault(b => b.ID == bikeId);
        if (_matchingBikeData == null)
        {
            Debug.Log("Bike not found in Bike list!");
            return;
        }
        
        // Instantiate the player bike
        GameManager.Instance.InGAME_PlayerBike = Instantiate(_matchingBikeData.BikePrefab, GameManager.Instance.playerObjectParent.transform);
        Debug.Log("Bike Instance: " + GameManager.Instance.InGAME_PlayerBike.ToString());
        // Assign
        CurrentBikeParticles = GameManager.Instance.InGAME_PlayerBike.GetComponent<BikeParticles>();
        CurrentBikeComponents = GameManager.Instance.InGAME_PlayerBike.GetComponent<BikeComponents>();

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
            Debug.Log("All bike components linked.");
        }

        if (GameManager.Instance.firstLaunch)
        {
            GameManager.Instance.InGAME_PlayerBike.SetActive(false);
            Debug.Log("First launch: " + GameManager.Instance.firstLaunch);
            GameManager.Instance.firstLaunch = false;
        }

        else
        {
            GameManager.Instance.InGAME_PlayerBike.SetActive(true);
        }

        // Load the trail as a child of the bike
        int _selectedTrailId = _playerData.SELECTED_TRAIL_ID;
        GameObject selectedTrail = TrailManager.Instance.GetTrailById(_selectedTrailId).TrailPrefab;

        if (selectedTrail != null)
        {
            // Instantiate the trail as a child of the bike
            currentTrailInstance = Instantiate(selectedTrail, GameManager.Instance.InGAME_PlayerBike.transform);

            // Find the Bike Trail empty GameObject in the bike prefab
            Transform bikeTrailTransform = GameManager.Instance.InGAME_PlayerBike.transform.Find("Bike Trail");
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

        Debug.Log("Bike Loaded: " + GameManager.Instance.InGAME_PlayerBike.ToString());

        // Set the game camera to follow the current bike instance
        CinemachineVirtualCamera virtualCamera = CameraController.Instance.GameCamera;
        virtualCamera.Follow = GameManager.Instance.InGAME_PlayerBike.transform;
    }

    public void LoadPlayerTrail(int trailId)
    {
        PlayerData data = SaveSystem.LoadPlayerData();
        if (!data.UNLOCKED_TRAILS.Contains(trailId))
        {
            Debug.Log("Trail not unlocked!");
            return;
        }

        // Destroy existing trail (if any)
        if (GameManager.Instance.InGAME_PlayerTrail != null)
        {
            Destroy(GameManager.Instance.InGAME_PlayerTrail);
        }

        // Find the TrailData with the matching trailId in the TrailDataList
        Trail matchingTrailData = GameManager.Instance.TrailList.FirstOrDefault(t => t.ID == trailId);
        if (matchingTrailData == null)
        {
            Debug.Log("Trail not found in TrailDataList!");
            return;
        }

        // Instantiate ---> Player Trail
        GameManager.Instance.InGAME_PlayerTrail = Instantiate(matchingTrailData.TrailPrefab, GameManager.Instance.InGAME_PlayerBike.transform);
        Debug.Log("Trail Instance: " + GameManager.Instance.InGAME_PlayerTrail.ToString());
    }

    private void HandleBike()
    {
        bool isGrounded = IsGrounded();
        if (isBeingPushedForward)
        {
            //rb.angularVelocity = 0; // Reset angular velocity
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

    private void CheckGroundContact()
    {
        if (Physics2D.IsTouchingLayers(bikeBodyCol, GameManager.Instance.GroundLayer))
        {
            if (Time.time - lastAirTime > flipDelay)
            {
                FaultFlip();
            }
        }
        else
        {
            lastAirTime = Time.time;
        }
    }

    private void HandleFlips()
    {
        bool isGrounded = IsGrounded();
        if (hasLanded)
        {
            hasLanded = false;
            if (internalFlipCount > 0)
            {
                flipCount += internalFlipCount;
                Debug.Log("Final Flip Count: " + flipCount);
                // SAVE DATA
                var _data = SaveSystem.LoadPlayerData();

                if (internalFlipCount > _data.BEST_INTERNAL_FLIPS)
                {
                    _data.BEST_INTERNAL_FLIPS = internalFlipCount;
                    SaveSystem.SavePlayerData(_data);
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
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
                //StatsJuice.PlayFeedbacks();
                Debug.Log("Internal Flips: " + internalFlipCount);
                // SAVE DATA
                var _data = SaveSystem.LoadPlayerData();
                _data.TOTAL_FLIPS += internalFlipCount;
                SaveSystem.SavePlayerData(_data);
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

    private void StartWheelie()
    {
        isWheelie = true;
        wheelieStartPosition = backWheelTransform.position;
        Invoke(nameof(BeginWheelie), wheelieGracePeriod);
    }

    private void BeginWheelie()
    {
        if (isWheelie) // In case the wheelie got cancelled before the grace period
        {
            wheelieStartTime = Time.time;

        }
    }

    private void PauseWheelie()
    {
        if (wheelieStartTime != 0)
        {
            float wheelieTime = Time.time - wheelieStartTime;
            wheelieStartTime = 0;
        }
    }
    
    private void EndWheelie()
    {
        if (isWheelie)
        {
            isWheelie = false;
            if (wheelieStartTime != 0)
            {
                float wheelieTime = Time.time - wheelieStartTime;
                float wheelieDistance = Vector2.Distance(wheelieStartPosition, backWheelTransform.position);
                wheeliePoints = wheelieTime * wheelieDistance;

                // SAVE DATA
                var _data = SaveSystem.LoadPlayerData();
                if (wheeliePoints > _data.BEST_SINGLE_WHEELIE)
                {
                    _data.BEST_SINGLE_WHEELIE = wheeliePoints;
                    // Show ingame notification!
                    // ... Notification System
                    // ....
                }
                _data.TOTAL_WHEELIE += wheeliePoints;
                SaveSystem.SavePlayerData(_data);

                GameManager.Instance.UpdateWheeliePoints(wheeliePoints);
                wheelieStartTime = 0; // reset
            }

            RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.GroundLayer);
            RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.GroundLayer);
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

    private void CheckSpeedBoost()
    {
        if (isSpeedBoosted && Time.time > speedBoostEndTime)
        {
            isSpeedBoosted = false;
            bikeMotorSpeed = currentMotorSpeed;
        }
    }

    private void HandleTrail()
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

    public float GetBikeSpeed()
    {
        if(bikeRb)
            return bikeRb.velocity.magnitude;
        else return 0;
    }

    public bool IsGrounded()
    {
        //RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        //return hitBack.collider != null || hitFront.collider != null;
        return bikeGroundCheckCol.IsTouchingLayers(GameManager.Instance.GroundLayer);
    }

    public bool IsRearWheelGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.GroundLayer);
        if(hitBack != false){
            return hitBack.collider != null;
        }
        return false;
    }

    public bool IsFrontWheelGrounded()
    {
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, bikeGroundCheckDistance, GameManager.Instance.GroundLayer);
        return hitFront.collider != null;
    }

    private void FaultFlip()
    {
        HapticPatterns.PlayConstant(0.70f, 0.55f, 0.1f); 
        faults++;
        GameManager.Instance.UpdateGameFaultCountText();

        // SAVE DATA
        var _data = SaveSystem.LoadPlayerData();
        _data.TOTAL_FAULTS += faults;

        // reset flip counter due to fault
        internalFlipCount = 0;

        SaveSystem.SavePlayerData(_data);

        // If a previous flip is still in progress
        if (currentFlickerCoroutine != null)
            StopCoroutine(currentFlickerCoroutine);

        var _playerBike = GameManager.Instance.InGAME_PlayerBike.gameObject.transform;
        RaycastHit2D groundHit = Physics2D.Raycast(_playerBike.position, -Vector2.up, Mathf.Infinity, GameManager.Instance.GroundLayer);

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

    public void ApplySpeedBoost(SpeedBoost.SpeedBoostData data)
    {
        if (!isBoosting)
        {
            isBoosting = true;
            boostMotorSpeed = bikeMotorSpeed + data.Amount;
            StartCoroutine(BoostCoroutine(data.Amount, data.Duration));
        }
    }

    public int GetFaultCount()
    {
        return faults;
    }

    public float CalculateLandingForce(float maxAirHeight, float currentHeight)
    {
        return maxAirHeight - currentHeight;
    }

    public IEnumerator SaveDistanceEveryFewSeconds(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            var _data = SaveSystem.LoadPlayerData();
            _data.TOTAL_DISTANCE += GetTotalDistanceInKilometers();
            SaveSystem.SavePlayerData(_data);
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
            Vector2 boostDirection = transform.right; 

            // Apply the boost force to the bike's rigidbody at the boost position
            bikeRb.AddForceAtPosition(boostDirection * amount * bikeRb.mass, boostPosition, ForceMode2D.Force);

            // Counteract the leaning effect by applying an opposite force
            Vector2 currentUpVector = transform.up;
            Vector2 leanForce = -Vector2.Dot(currentUpVector, initialUpVector) * transform.forward * amount * bikeRb.mass;
            bikeRb.AddForceAtPosition(leanForce, boostPosition, ForceMode2D.Force);

            // Apply limited rotation force during the boost
            float rotationForceMultiplier = isSpeedBoosted ? 0.3f : 1f;
            bikeRb.AddTorque(-flipTorque * rotationForceMultiplier);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBoosting = false;
        bikeMotorSpeed = boostMotorSpeed; // Set the motor speed to the boosted speed
        isSpeedBoosted = false;
    }

    private IEnumerator RotateBikeToFaceForward(float duration)
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

    public Bike[] GetAllBikes()
    {
        return BikeList;
    }

    public Bike GetBikeById(int id)
    {
        return BikeList.FirstOrDefault(b => b.ID == id);
    }

    public BikeComponents GetCurrentBikeComponents()
    {
        if (GameManager.Instance.InGAME_PlayerBike != null)
            return CurrentBikeComponents;
        else 
            Debug.LogWarning("No bike components were found.");
            return null; 
    }
}