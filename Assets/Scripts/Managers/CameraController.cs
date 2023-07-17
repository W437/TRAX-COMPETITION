using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance;

    private float transitionDuration = 1.5f;
    public CinemachineVirtualCamera GameCamera;
    public CinemachineVirtualCamera MenuCamera;
    public CinemachineVirtualCamera ShopCamera;
    public CinemachineVirtualCamera SettingsCamera;
    // Target values
    private float targetOrthographicSize;
    private float targetScreenX;
    private float transitionTime;
    // Reference to the VirtualCamera
    public CinemachineVirtualCamera MainVirtualCamera;
    public Camera mainCamera;
    // Reference to the Composer
    private CinemachineFramingTransposer mainComposer, menuComposer;
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
        mainComposer = MainVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        menuComposer = MenuCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        // Set initial values
        targetOrthographicSize = MainVirtualCamera.m_Lens.OrthographicSize;
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
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
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
                else if (!isJumping)
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
                MainVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(MainVirtualCamera.m_Lens.OrthographicSize, targetOrthographicSize, progress);
                mainComposer.m_ScreenX = Mathf.Lerp(mainComposer.m_ScreenX, targetScreenX, progress);
            }
            else
            {
                // Set to target to prevent tiny fluctuations due to math precision
                MainVirtualCamera.m_Lens.OrthographicSize = targetOrthographicSize;
                mainComposer.m_ScreenX = targetScreenX;
            }

            // Update the jumping state for the next frame
            wasJumping = isJumping;

        }
    }


    private void SetCameraBlendStyle(CinemachineVirtualCamera fromCam, CinemachineVirtualCamera toCam)
    {
        if ((fromCam == GameCamera && toCam == MenuCamera) || fromCam == MenuCamera && toCam == GameCamera)
        {
            CinemachineBlendDefinition blendDef = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0);
            mainCameraBrain.m_DefaultBlend = blendDef;
            StartCoroutine(RevertDefaultBlend());
        }
        else if ((fromCam == (ShopCamera || SettingsCamera)) && toCam == MenuCamera || fromCam == MenuCamera && toCam == ShopCamera)
        {
            mainCameraBrain.m_DefaultBlend = originalBlendDefinition;
        }
    }

    public void SwitchToGameCamera()
    {
        SetCameraBlendStyle(MenuCamera, GameCamera);
        GameCamera.Priority = 11;
        MenuCamera.Priority = 10;
    }

    public void SwitchToMenuCamera()
    {
        if (GameCamera.Priority > MenuCamera.Priority)
        {
            SetCameraBlendStyle(GameCamera, MenuCamera);
        }
        else if (ShopCamera.Priority > MenuCamera.Priority)
        {
            SetCameraBlendStyle(ShopCamera, MenuCamera);
        }
        GameCamera.Priority = 10;
        MenuCamera.Priority = 11;
        ShopCamera.Priority = 9;
        SettingsCamera.Priority = 8;
    }

    public void SwitchToShopCamera()
    {
        SetCameraBlendStyle(MenuCamera, ShopCamera);
        ShopCamera.Priority = 11;
        MenuCamera.Priority = 10;
        GameCamera.Priority = 9;
        SettingsCamera.Priority = 8;
    }

    public void SwitchToSettingsCamera()
    {
        SetCameraBlendStyle(MenuCamera, SettingsCamera);
        SettingsCamera.Priority = 11;
        MenuCamera.Priority = 10;
        ShopCamera.Priority = 9;
        GameCamera.Priority = 8;
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

    private IEnumerator RevertDefaultBlend()
    {
        yield return new WaitForSeconds(1);
        mainCameraBrain.m_DefaultBlend = originalBlendDefinition;
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

