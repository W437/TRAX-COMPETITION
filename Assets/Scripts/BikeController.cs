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

    public int flipCount = 0; // Flip counter
    private float lastZRotation = 0f;
    private float rotationCounter = 0;

    private bool isWheelie = false;
    private float wheelieStartTime = 0f;

    // Wheelie counter
    private float wheelieGracePeriod = 0.33f; // in seconds
    private float wheelieGraceEndTime;


    // Speed boost
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;
    public float normalMotorSpeed;

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
    }

    private void Update()
    {
        bool isGrounded = IsGrounded();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            accelerationStartTime = Time.time;
            wj.useMotor = true;
            //Debug.Log("Accelerating");
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded)
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

        if (Input.GetKeyUp(KeyCode.Space))
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
                // Check if the bike landed upside down
                if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
                {
                    Debug.Log("Bike landed upside down. Wheelie not counted.");
                    wheelieStartTime = 0;
                }
                else
                {
                    isWheelie = false;
                    float wheelieTime = Time.time - wheelieStartTime;
                    Debug.Log("Wheelie time: " + wheelieTime + " seconds");
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

        // check for speed boost
        if (isSpeedBoosted && Time.time > speedBoostEndTime)
        {
            isSpeedBoosted = false;
            motorSpeed = currentMotorSpeed;
        }

        // Rest of the code...
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
        // Set the bike to an upright position
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Slightly lift up the bike to avoid intersection with the ground
        transform.position += new Vector3(0, 0.3f, 0);
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

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBoosting = false;
        motorSpeed = boostMotorSpeed; // Set the motor speed to the boosted speed
        isSpeedBoosted = false;
    }


    private void FixedUpdate()
    {
        // Apply a downward force to the car
        //rb.AddForce(Vector2.down * downwardForce, ForceMode2D.Force);
    }
}
