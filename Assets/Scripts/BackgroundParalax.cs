using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BackgroundParalax : MonoBehaviour
{
    public static BackgroundParalax Instance;
    public Transform playerCam;
    public Transform[] backgrounds;
    public Transform overlay;

    public float startSensitivity = 0;
    public float endSensitivity = 0.4f;
    public float horizontalSpawnOffsetUnits = 5f; // 500 pixels converted to Unity units

    public Transform sun;
    private Transform finishLine;
    private float totalDistance;
    private Vector3 initialSunPosition;
    private Vector3 playerStartPosition;
    private Camera playerCamera;

    private Vector2 lastPos = Vector2.zero;
    private Vector2 delta = Vector2.zero;
    private float initialPlayerY;

    private List<LinkedList<SpriteRenderer>> backgroundLayers;

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        playerCamera = playerCam.GetComponent<Camera>();
        Debug.Log("Paralax Init");
        lastPos = playerCam.position;
        initialPlayerY = playerCam.position.y-5;
        backgroundLayers = new List<LinkedList<SpriteRenderer>>(backgrounds.Length);

        for (int i = 0; i < backgrounds.Length; ++i)
        {
            var child = backgrounds[i].GetChild(0);
            var childTransform = child.transform;

            var newObj = Instantiate(child.gameObject, childTransform.position, childTransform.rotation);

            var list = new LinkedList<SpriteRenderer>();
            list.AddFirst(newObj.GetComponent<SpriteRenderer>());

            backgroundLayers.Add(list);

            backgrounds[i].gameObject.SetActive(false);
        }

        spawnInView();
    }

    /// <summary>
    /// Return random item from background collection at specific layer
    /// </summary>
    GameObject getRandomSpriteAtLayer(int layer)
    {
        return backgrounds[layer].GetChild(Random.Range(0, backgrounds[layer].childCount)).gameObject;
    }

    void Update()
    {
        updateDelta();

        matchOverlayWithCamera();

        moveParalax();

        spawnInView();

        clearInvisible();

        UpdateSunPosition();
    }

    private void UpdateSunPosition()
    {
        if (finishLine != null)
        {
            // Calculate the distance the player has moved from the starting position along the X axis
            float distanceMoved = Mathf.Abs(playerCam.position.x - playerStartPosition.x);

            // Get a normalized value representing how far the player has moved towards the finish
            float progress = Mathf.Clamp01(distanceMoved / totalDistance);

            // Convert the sun's position to viewport space
            Vector3 sunViewportPosition = playerCamera.WorldToViewportPoint(sun.position);

            // Set the sun's new viewport x position based on the progress
            sunViewportPosition.x = 1 - progress;

            // Convert the sun's viewport position back to world space, keeping a fixed Z value
            Vector3 newSunPosition = playerCamera.ViewportToWorldPoint(sunViewportPosition);
            newSunPosition.z = sun.position.z;

            // Update the sun's position
            sun.position = newSunPosition;
        }
    }

    public void SetFinishLine(Transform finishLineTransform)
    {
        finishLine = finishLineTransform;
        CalculateTotalDistance();
    }

    private void CalculateTotalDistance()
    {
        if (finishLine != null)
        {
            totalDistance = Mathf.Abs(finishLine.position.x - playerCam.position.x);
            playerStartPosition = playerCam.position;
            initialSunPosition = sun.position;
            Debug.Log("Sun finish distance: " + totalDistance );
        }
    }

    private void clearInvisible()
    {
        foreach (var backgroundLayer in backgroundLayers)
        {
            var node = backgroundLayer?.First;
            while (node != null)
            {
                var next = node.Next;
                if (CameraUtils.outOfView(node.Value, playerCam, 1) && backgroundLayer.Count > 1)
                {
                    Destroy(node.Value.gameObject);
                    backgroundLayer.Remove(node);
                }

                node = next;
            }
        }
    }

    private void spawnInView()
    {
        for (int i = 0; i < backgroundLayers.Count; ++i)
        {
            var sprite = backgroundLayers[i].Last;

            //the right edge of right-most sprite is visible - need to spawn to the right
            if (CameraUtils.rightEdgeIn(sprite.Value, playerCam))
            {
                var newObject = spawnSpriteObj(sprite.Value, getRandomSpriteAtLayer(i), 1, i);
                backgroundLayers[i].AddLast(newObject.GetComponent<SpriteRenderer>());
            }

            sprite = backgroundLayers[i].First;
            //the left edge of right-most sprite is visible - need to spawn to the left
            if (CameraUtils.leftEdgeIn(sprite.Value, playerCam))
            {
                var newObject = spawnSpriteObj(sprite.Value, getRandomSpriteAtLayer(i), -1, i);
                backgroundLayers[i].AddFirst(newObject.GetComponent<SpriteRenderer>());
            }
        }
    }

    private void moveParalax()
    {
        for (int i = 0; i < backgroundLayers.Count; ++i)
        {
            //calculate parallax weight linearly based on how "far" the layer is
            //a simple way to move each layer with different speed
            float shiftWeight = startSensitivity + (endSensitivity - startSensitivity) * i / (backgroundLayers.Count - 1);

            foreach (var sprite in backgroundLayers[i])
            {
                //offset the sprite with weight
                sprite.transform.Translate(delta * shiftWeight);
            }
        }
    }

    public void ResetParallax()
    {
        // Clear the background layers
        foreach (var backgroundLayer in backgroundLayers)
        {
            foreach (var spriteRenderer in backgroundLayer)
            {
                Destroy(spriteRenderer.gameObject);
            }
            backgroundLayer.Clear();
            clearInvisible();
            Debug.Log("Paralax Reset");
        }

        // Reset the parallax
        Initialize();
    }


    private void updateDelta()
    {
        delta = (Vector2)playerCam.position - lastPos;

        lastPos = playerCam.position;
    }

    private void matchOverlayWithCamera()
    {
        for (int i = 0; i < overlay.childCount; ++i)
        {
            var child = overlay.GetChild(i);

            var render = child.GetComponent<SpriteRenderer>();
            var size = render.bounds.size;
            var cameraSize = new Vector3(CameraUtils.getCamWidth(playerCam) * 2, CameraUtils.getCamHeight(playerCam) * 2, 0);

            var scaleFactor = new Vector3(cameraSize.x / size.x, cameraSize.y / size.y, 1);

            child.localScale = Vector3.Scale(child.localScale, scaleFactor);
            child.localPosition = new Vector3(0, 0, 0);
        }
    }

    private GameObject spawnSpriteObj(SpriteRenderer sample, GameObject sprite, float shift = 1.0f, int layer = 0)
    {
        return spawnSprite(sample, sprite.GetComponent<SpriteRenderer>(), shift, layer);
    }

    private GameObject spawnSprite(SpriteRenderer sample, SpriteRenderer sprite, float shift, int layer)
    {
        var sampleTransform = sample.transform;
        var newObject = Instantiate(sprite.gameObject, sampleTransform.position, sprite.transform.rotation);
        var position = newObject.transform.position;

        // Check if the player's current Y position is within the acceptable range of +-10 units
        float newY = Mathf.Abs(playerCam.position.y - initialPlayerY) <= 10 ? initialPlayerY : sprite.transform.position.y;

        newObject.transform.position = new Vector3(position.x, newY, position.z);
        newObject.SetActive(true);

        // Adjusting the x-axis translation by the offset
        newObject.transform.Translate((shift * (CameraUtils.getWidth(sprite) + CameraUtils.getWidth(sample) + getExtraOffset(layer))) + (horizontalSpawnOffsetUnits * shift), 0, 0, Space.World);
        return newObject;
    }


    protected virtual float getExtraOffset(int id)
    {
        return 0;
    }

}
