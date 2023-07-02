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
    public CinemachineVirtualCamera settingsCamera;
    // Target values

    float targetOrthographicSize;
    float targetScreenX;

    // Time passed since transition start
    float transitionTime;

    // Reference to the VirtualCamera
    public CinemachineVirtualCamera virtualCamera;
    public Camera mainCamera;

    // Reference to the Composer
    public CinemachineFramingTransposer mainComposer, menuComposer;

    private CinemachineBrain mainCameraBrain;
    private CinemachineBlendDefinition originalBlendDefinition;

    // Previous jumping state
    bool wasJumping;

    float velocityCheckDelay = 0.2f;
    bool checkingVelocity = false;

    void Awake()
    {
        Instance = this;
        mainCameraBrain = mainCamera.GetComponent<CinemachineBrain>();
        originalBlendDefinition = mainCameraBrain.m_DefaultBlend;
    }

    void Start()
    {
        mainComposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        menuComposer = menuCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        // Set initial values
        targetOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
        targetScreenX = mainComposer.m_ScreenX;

        // Initialize transition time
        transitionTime = transitionDuration;
        var _gameManager = GameManager.Instance;
        // Initialize jumping state
        if (_gameManager.gameState == GameManager.GameState.Playing)
            wasJumping = !BikeController.Instance.IsGrounded();

        Random.InitState((int)Random.Range(0f, 100f));
    }

    void Update()
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
                mainComposer.m_ScreenX = Mathf.Lerp(mainComposer.m_ScreenX, targetScreenX, progress);
            }
            else
            {
                // Set to target to prevent tiny fluctuations due to math precision
                virtualCamera.m_Lens.OrthographicSize = targetOrthographicSize;
                mainComposer.m_ScreenX = targetScreenX;
            }

            // Update the jumping state for the next frame
            wasJumping = isJumping;

        }
    }

    public IEnumerator AnimateScreenX(float startScreenX, float endScreenX, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentScreenX = Mathf.Lerp(startScreenX, endScreenX, elapsedTime / duration);
            menuComposer.m_ScreenX = currentScreenX;

            yield return null;
        }

        // Ensure the ScreenX ends up at the exact target value
        menuComposer.m_ScreenX = endScreenX;
    }

    private void SetCameraBlendStyle(CinemachineVirtualCamera fromCam, CinemachineVirtualCamera toCam)
    {
        if (fromCam == gameCamera && toCam == menuCamera)
        {
            CinemachineBlendDefinition blendDef = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0);
            mainCameraBrain.m_DefaultBlend = blendDef;
            StartCoroutine(RevertDefaultBlend());
        }
        else if ((fromCam == (shopCamera || settingsCamera)) && toCam == menuCamera)
        {
            mainCameraBrain.m_DefaultBlend = originalBlendDefinition;
        }
    }

    IEnumerator RevertDefaultBlend()
    {
        yield return new WaitForSeconds(1);
        mainCameraBrain.m_DefaultBlend = originalBlendDefinition;    
    }

    public void SwitchToGameCamera()
    {
        SetCameraBlendStyle(menuCamera, gameCamera);
        gameCamera.Priority = 11;
        menuCamera.Priority = 10;
    }

    public void SwitchToMenuCamera()
    {
        if (gameCamera.Priority > menuCamera.Priority)
        {
            SetCameraBlendStyle(gameCamera, menuCamera);
        }
        else if (shopCamera.Priority > menuCamera.Priority)
        {
            SetCameraBlendStyle(shopCamera, menuCamera);
        }
        gameCamera.Priority = 10;
        menuCamera.Priority = 11;
        shopCamera.Priority = 9;
        settingsCamera.Priority = 8;
    }

    public void SwitchToShopCamera()
    {
        SetCameraBlendStyle(menuCamera, shopCamera);
        shopCamera.Priority = 11;
        menuCamera.Priority = 10;
        gameCamera.Priority = 9;
        settingsCamera.Priority = 8;
    }

    public void SwitchToSettingsCamera()
    {
        SetCameraBlendStyle(menuCamera, settingsCamera);
        settingsCamera.Priority = 11;
        menuCamera.Priority = 10;
        shopCamera.Priority = 9;
        gameCamera.Priority = 8;
    }



    IEnumerator DelayedVelocityCheck(bool jumped)
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

