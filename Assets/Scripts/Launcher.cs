using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float launchForce = 10f; 
    public float launchDelay = 1f; 
    public enum LaunchPoint { LeftPivot, Center, RightPivot }
    public LaunchPoint launchPoint;
    private bool flag = false;
    public Rigidbody2D playerRb;
    private bool isLaunching = false;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!flag)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    StartCoroutine(LaunchPlayer(playerRb));
                }
                flag = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isLaunching)
        {
            playerRb.angularVelocity = Mathf.Clamp(playerRb.angularVelocity, -1, 1);
        }
    }


    private IEnumerator LaunchPlayer(Rigidbody2D playerRb)
    {
        isLaunching = true;

        // Remember the initial rotation
        Quaternion initialRotation = playerRb.transform.rotation;

        yield return new WaitForSeconds(launchDelay);

        // Calculate launch direction based on selected launch point
        Vector2 launchDirection = Vector2.up;
        switch (launchPoint)
        {
            case LaunchPoint.LeftPivot:
                launchDirection = Quaternion.Euler(0, 0, 45) * Vector2.up;
                break;
            case LaunchPoint.Center:
                launchDirection = Vector2.up;
                break;
            case LaunchPoint.RightPivot:
                launchDirection = Quaternion.Euler(0, 0, -45) * Vector2.up;
                break;
        }

        playerRb.velocity = launchDirection * launchForce;

        // Reset the angular velocity and set the rotation to initial rotation
        playerRb.angularVelocity = 0;
        playerRb.transform.rotation = initialRotation;

        yield return new WaitForSeconds(0.1f);
        isLaunching = false;
    }





}
