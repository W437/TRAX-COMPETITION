using UnityEngine;

public class BikeParticles : MonoBehaviour
{
    public static BikeParticles Instance;

    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private ParticleSystem landingParticles;
    [SerializeField] private Rigidbody2D rearWheelRigidbody;
    [SerializeField] private Rigidbody2D frontWheelRigidbody;
    [SerializeField] private Rigidbody2D bikeRigidbody;
    [SerializeField] private Transform rearWheel;
    [SerializeField] private LayerMask groundLayer;
    private float speedThreshold = 0.2f;
    private float minEmissionRate = 35f;
    private float maxEmissionRate = 100f;

    private float minParticleSpeed = 5f; 
    private float maxParticleSpeed = 10f;

    public float minParticleSize = 0.5f; 
    public float maxParticleSize = 4f;

    private float maxBikeSpeed;
    private float maxAirHeight;

    private bool isFrontWheelTouchingGround = false;
    private bool isBackWheelTouchingGround = false;

    private void Awake()
    {
        Instance = this;
    }
 
    private void Start()
    {
        Debug.Log(BikeController.Instance.CalculateMaxSpeed());
        maxBikeSpeed = 10f;
    }

    void Update()
    {
        // Cast a ray downwards from the rear wheel to find the ground.
        RaycastHit2D rearHit = Physics2D.Raycast(rearWheel.position, -Vector2.up, Mathf.Infinity, groundLayer);
        RaycastHit2D bikeHit = Physics2D.Raycast(bikeRigidbody.position, -Vector2.up, Mathf.Infinity, groundLayer);


        if (rearHit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            dirtParticles.transform.position = rearHit.point + Vector2.up * 0.05f;
        }

        if (rearHit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            landingParticles.transform.position = bikeHit.point + Vector2.up * 0.1f;
        }

        float speed = rearWheelRigidbody.velocity.x;
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
        //GameManager.Instance.airtime
        return !BikeController.Instance.IsRearWheelGrounded();
        // Implement this method to check if the bike is in the air.
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


}
