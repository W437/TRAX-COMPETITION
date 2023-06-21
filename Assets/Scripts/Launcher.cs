using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float launchForce = 10f; // The force with which the player will be launched
    public float launchDelay = 1f; // The delay after which the player will be launched
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
        // Set isLaunching to true
        isLaunching = true;

        // Remember the initial rotation
        Quaternion initialRotation = playerRb.transform.rotation;

        // Wait for the specified delay
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

        // Set the velocity of the player's Rigidbody
        playerRb.velocity = launchDirection * launchForce;

        // Reset the angular velocity and set the rotation to initial rotation
        playerRb.angularVelocity = 0;
        playerRb.transform.rotation = initialRotation;

        // Wait for a short delay before setting isLaunching to false
        yield return new WaitForSeconds(0.1f);
        isLaunching = false;
    }





}
