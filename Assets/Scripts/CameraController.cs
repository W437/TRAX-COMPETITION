using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance;
    // Duration of the transition
    public float transitionDuration = 1.5f;

    public CinemachineVirtualCamera gameCamera;
    public CinemachineVirtualCamera menuCamera;
    public CinemachineVirtualCamera shopCamera;
    // Target values
    private float targetOrthographicSize;
    private float targetScreenX;

    // Time passed since transition start
    private float transitionTime;

    // Reference to the VirtualCamera
    public CinemachineVirtualCamera virtualCamera;

    // Reference to the Composer
    public CinemachineFramingTransposer composer;

    // Previous jumping state
    private bool wasJumping;

    private float velocityCheckDelay = 0.2f;
    private bool checkingVelocity = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        composer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        // Set initial values
        targetOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
        targetScreenX = composer.m_ScreenX;

        // Initialize transition time
        transitionTime = transitionDuration;

        // Initialize jumping state
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            wasJumping = !BikeController.Instance.IsGrounded();

        Random.InitState((int)Random.Range(0f, 100f));
    }

    private void Update()
    {
        if(GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            // Check if the bike is in the air
            bool isJumping = !BikeController.Instance.IsGrounded();

            // If jumping state has changed, update the targets and reset the transition
            if (isJumping != wasJumping)
            {
                // Set the target based on the bike's state
                if (!checkingVelocity)
                {
                    StartCoroutine(DelayedVelocityCheck(isJumping));
                }
                else if(!isJumping)
                {
                    targetOrthographicSize = 5.5f;
                    targetScreenX = 0.13f;
                }

                // Reset transition
                transitionTime = 0f;
            }

            if (transitionTime < transitionDuration)
            {
                // Increase time passed
                transitionTime += Time.deltaTime;

                // Calculate eased progress
                float progress = transitionTime / transitionDuration;

                // Apply ease-in-out function based on the current state
                if (isJumping)
                {
                    // Cubic ease-out function
                    progress = -0.5f * (Mathf.Cos(Mathf.PI * progress) - 1);
                }
                else
                {
                    // Cubic ease-in function
                    progress = Mathf.Pow(progress, 3);
                }

                // Apply smooth changes to camera
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetOrthographicSize, progress);
                composer.m_ScreenX = Mathf.Lerp(composer.m_ScreenX, targetScreenX, progress);
            }
            else
            {
                // Set to target to prevent tiny fluctuations due to math precision
                virtualCamera.m_Lens.OrthographicSize = targetOrthographicSize;
                composer.m_ScreenX = targetScreenX;
            }

            // Update the jumping state for the next frame
            wasJumping = isJumping;

        }
    }

    public void SwitchToGameCamera()
    {
        gameCamera.Priority = 11;
        menuCamera.Priority = 10;
    }

    public void SwitchToMenuCamera()
    {
        gameCamera.Priority = 10;
        menuCamera.Priority = 11;
        shopCamera.Priority = 9;
    }

    public void SwitchToShopCamera()
    {
        shopCamera.Priority = 11;
        menuCamera.Priority = 10;
        shopCamera.Follow = ScreenManager.Instance.RB_MenuBike.transform;
    }

    private IEnumerator DelayedVelocityCheck(bool jumped)
    {
        checkingVelocity = true;

        // Wait for the specified delay
        yield return new WaitForSeconds(velocityCheckDelay + Random.Range(-0.05f, 0.1f));

        // Only start zooming out if bike is moving upwards
        if (jumped && BikeController.Instance.GetVerticalVelocity() > 0)
        {
            targetOrthographicSize = 6.85f;
            targetScreenX = 0.40f;

            // Reset transition
            transitionTime = 0f;
        }
        else if (!jumped)
        {
            targetOrthographicSize = 5.5f;
            targetScreenX = 0.13f;

            // Reset transition
            transitionTime = 0f;
        }

        checkingVelocity = false;
    }
}

