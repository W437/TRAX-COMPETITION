using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BikeController : MonoBehaviour
{
    public WheelJoint2D backWheel;
    public WheelJoint2D frontWheel;
    public Transform backWheelTransform;
    public Transform frontWheelTransform;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float motorSpeed;
    public float maxTorque;
    public float downwardForce;
    public float accelerationTime;
    public float flipTorque;
    public float doublePressTime = 0.3f;
    public float flipDelay = 1f; // time in seconds to wait before flipping the bike
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
            Debug.Log("Accelerating");
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
                rb.AddTorque(flipTorque);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            wj.useMotor = false;
            Debug.Log("Stop Motor");
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
    }

    private bool IsGrounded()
    {
        RaycastHit2D hitBack = Physics2D.Raycast(backWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        RaycastHit2D hitFront = Physics2D.Raycast(frontWheelTransform.position, -Vector2.up, groundCheckDistance, groundLayer);
        return hitBack.collider != null || hitFront.collider != null;
    }

    private void Flip()
    {
        // Set the bike to an upright position
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Slightly lift up the bike to avoid intersection with the ground
        transform.position += new Vector3(0, 0.5f, 0);
    }

    private void FixedUpdate()
    {
        // Apply a downward force to the car
        //rb.AddForce(Vector2.down * downwardForce, ForceMode2D.Force);
    }
}
