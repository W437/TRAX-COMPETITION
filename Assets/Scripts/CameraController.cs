using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float maxZoomOutHeight;
    public float normalFOV = 60;
    public float maxFOV = 90;
    public LayerMask groundLayer;
    private Coroutine zoomCoroutine;
    private Coroutine checkAirborneHeightCoroutine;
    private bool wasGrounded;

    private void Update()
    {
        if(GameManager.Instance.gameState == GameState.Playing)
        {
            bool grounded = BikeController.Instance.IsGrounded();
            //Debug.Log("grounded?: " + grounded);
            RaycastHit hit;
            Vector3 raycastDirection = -transform.up;

            if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity, groundLayer))
            {
                Debug.DrawRay(transform.position, raycastDirection * hit.distance, Color.red, 5f);

                if (!grounded)
                {
                    if (checkAirborneHeightCoroutine != null)
                    {
                        StopCoroutine(checkAirborneHeightCoroutine);
                    }
                    checkAirborneHeightCoroutine = StartCoroutine(CheckAirborneHeight(hit.distance));
                }
            }

            if (grounded && zoomCoroutine == null)
            {
                if (checkAirborneHeightCoroutine != null)
                {
                    StopCoroutine(checkAirborneHeightCoroutine);
                    checkAirborneHeightCoroutine = null;
                }
                zoomCoroutine = StartCoroutine(ZoomInCoroutine());
            }

            wasGrounded = grounded;
        }
    }

    IEnumerator CheckAirborneHeight(float distanceToGround)
    {
        yield return new WaitForSeconds(0.2f);
        float heightRatio = Mathf.Clamp01(distanceToGround / maxZoomOutHeight);
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(normalFOV, maxFOV, heightRatio);
    }

    IEnumerator ZoomInCoroutine()
    {
        float elapsedTime = 0f;
        float zoomInDuration = 0.5f;
        float initialFOV = virtualCamera.m_Lens.FieldOfView;
        while (elapsedTime < zoomInDuration)
        {
            elapsedTime += Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(initialFOV, normalFOV, elapsedTime / zoomInDuration);
            yield return null;
        }
        zoomCoroutine = null;
    }
}
