using System.Collections;
using UnityEngine;

public class BikeParticles : MonoBehaviour
{
    public static BikeParticles Instance;

    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private ParticleSystem landingParticles;
    [SerializeField] private LayerMask groundLayer;
    public BikeComponents bikeComponents;


    private float speedThreshold = 0.2f;
    private float minEmissionRate = 35f;
    private float maxEmissionRate = 100f;

    private float minParticleSpeed = 5f; 
    private float maxParticleSpeed = 10f;

    public float minParticleSize = 0.5f; 
    public float maxParticleSize = 4f;

    private float maxBikeSpeed;
    private float maxAirHeight;

    private void Awake()
    {
        Instance = this;
        BikeController.Instance.OnPlayerBikeChanged += HandlePlayerBikeChanged;
    }
 
    private void Start()
    {
        maxBikeSpeed = 10f;

    }

    void Update()
    {
        if (bikeComponents == null)
        {
            Debug.LogError("BikeParticles: BikeComponents is null!");
            return;
        }

        // Cast a ray downwards from the rear wheel to find the ground.
        RaycastHit2D rearHit = Physics2D.Raycast(bikeComponents.BackWheelTransform.position, -Vector2.up, Mathf.Infinity, groundLayer);
        RaycastHit2D bikeHit = Physics2D.Raycast(bikeComponents.RB_Bike.position, -Vector2.up, Mathf.Infinity, groundLayer);

        if (rearHit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            dirtParticles.transform.position = rearHit.point + Vector2.up * 0.05f;
        }

        if (bikeHit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            landingParticles.transform.position = bikeHit.point + Vector2.up * 0.1f;
        }

        float speed = bikeComponents.RB_BackWheel.velocity.x;
        float speedRatio = Mathf.Clamp01(speed / maxBikeSpeed);

        var emission = dirtParticles.emission;
        var main = dirtParticles.main;

        if (speed > speedThreshold && !IsInAir())
        {
            float emissionRate = Mathf.Lerp(minEmissionRate, speedRatio * maxEmissionRate, speedRatio);

            emission.rateOverTime = new ParticleSystem.MinMaxCurve(minEmissionRate, emissionRate);

            float startSpeed = Mathf.Lerp(minParticleSpeed, speedRatio * maxParticleSpeed, speedRatio);
            main.startSpeed = new ParticleSystem.MinMaxCurve(minParticleSpeed, startSpeed);

            float startSize = Mathf.Lerp(minParticleSize, speedRatio * maxParticleSize, speedRatio);
            main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, startSize);

            // Enable emission when the bike is on the ground.
            if (!emission.enabled)
                emission.enabled = true;
        }
        else
        {
            emission.rateOverTime = minEmissionRate;
            main.startSpeed = minParticleSpeed;
            main.startSize = minParticleSize;

            // Disable emission when the bike is in the air.
            if (emission.enabled)
                emission.enabled = false;
        }
    }


    bool IsInAir()
    {
        return !BikeController.Instance.IsRearWheelGrounded();
    }

    public float CalculateLandingForce(float maxAirHeight, float currentHeight)
    {
        return maxAirHeight - currentHeight;
    }

    public void PlayLandingParticles(float landingForce)
    {
        // Set the particle size and ratio over time
        var main = landingParticles.main;
        main.startSize = landingForce * 0.05f; 
        main.startLifetime = landingForce * 0.05f;

        // Set the emission rate based on the landing force
        var emission = landingParticles.emission;
        emission.rateOverTime = landingForce * 10;

        // Play the particles
        landingParticles.Stop();
        landingParticles.Play();
    }


    private void OnDestroy()
    {
        BikeController.Instance.OnPlayerBikeChanged -= HandlePlayerBikeChanged;
    }

    private void OnEnable()
    {
        BikeController.Instance.OnPlayerBikeChanged += HandlePlayerBikeChanged;
    }

    private void OnDisable()
    {
        BikeController.Instance.OnPlayerBikeChanged -= HandlePlayerBikeChanged;
    }

    public void HandlePlayerBikeChanged()
    {
        StartCoroutine(WaitForBikeComponents());
    }

    private IEnumerator WaitForBikeComponents()
    {
        // Wait until the bike components are initialized
        while (BikeController.Instance.GetCurrentBikeComponents() == null)
        {
            yield return null;
        }

        bikeComponents = BikeController.Instance.GetCurrentBikeComponents();
    }

    private IEnumerator InitializeBikeComponents()
    {
        // Wait for the next frame to ensure the bike prefab is instantiated
        yield return null;

        if (BikeController.Instance.PlayerBike != null)
        {
            bikeComponents = BikeController.Instance.PlayerBike.GetComponentInChildren<BikeComponents>();
        }
        else
        {
            bikeComponents = null;
        }

        // Do anything else you need to do when PlayerBike changes
    }

}
