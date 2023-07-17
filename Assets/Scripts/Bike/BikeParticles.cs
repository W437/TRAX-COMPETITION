using Lofelt.NiceVibrations;
using UnityEngine;

public class BikeParticles : MonoBehaviour
{
    private ParticleSystem dirtParticles;
    private ParticleSystem landingParticles;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BikeComponents bikeComponents;


    private float speedThreshold = 0.2f;
    private float minEmissionRate = 35f;
    private float maxEmissionRate = 100f;

    public float MinParticleSpeed;
    public float MaxParticleSpeed;

    public float MinParticleSize;
    public float MaxParticleSize;

    private float maxBikeSpeed;
    private float maxAirHeight;
    private RaycastHit2D rearHit;
    private RaycastHit2D bikeHit;


    private void Start()
    {
        maxBikeSpeed = 10f;
    }

    void Update()
    {
        maxAirHeight = Mathf.Max(maxAirHeight, transform.position.y);

        rearHit = Physics2D.Raycast(BikeController.Instance.CurrentBikeComponents.BackWheelTransform.position, -Vector2.up, Mathf.Infinity, groundLayer);
        bikeHit = Physics2D.Raycast(BikeController.Instance.CurrentBikeComponents.RB_Bike.position, -Vector2.up, Mathf.Infinity, groundLayer);

        if (rearHit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            BikeController.Instance.CurrentBikeComponents.DirtParticles.transform.position = rearHit.point + Vector2.up * 0.05f;
        }

        if (bikeHit.collider != null)
        {
            BikeController.Instance.CurrentBikeComponents.LandingParticles.transform.position = bikeHit.point + Vector2.up * 0.1f;
        }

        float speed = BikeController.Instance.CurrentBikeComponents.RB_BackWheel.velocity.x;
        float speedRatio = Mathf.Clamp01(speed / maxBikeSpeed);


        var emission = BikeController.Instance.CurrentBikeComponents.DirtParticles.emission;
        var main = BikeController.Instance.CurrentBikeComponents.DirtParticles.main;

        bool isGrounded = BikeController.Instance.IsGrounded();

        if (speed > speedThreshold && isGrounded)
        {
            float emissionRate = Mathf.Lerp(minEmissionRate, speedRatio * maxEmissionRate, speedRatio);

            emission.rateOverTime = new ParticleSystem.MinMaxCurve(minEmissionRate, emissionRate);

            float startSpeed = Mathf.Lerp(MinParticleSpeed, speedRatio * MaxParticleSpeed, speedRatio);
            main.startSpeed = new ParticleSystem.MinMaxCurve(MinParticleSpeed, startSpeed);

            float startSize = Mathf.Lerp(MinParticleSize, speedRatio * MaxParticleSize, speedRatio);
            main.startSize = new ParticleSystem.MinMaxCurve(MinParticleSize, startSize);

            // Enable emission when the bike is on the ground.
            if (!emission.enabled)
                emission.enabled = true;
        }
        else
        {
            emission.rateOverTime = minEmissionRate;
            main.startSpeed = MinParticleSpeed;
            main.startSize = MinParticleSize;

            // Disable emission when the bike is in the air.
            if (emission.enabled)
                emission.enabled = false;
        }
    }


    public void PlayLandingParticles(float landingForce)
    {
        // Set the particle size and ratio over time
        var _landingParticles = BikeController.Instance.CurrentBikeComponents.LandingParticles;
        var main = _landingParticles.main;
        main.startSize = landingForce * 0.05f;
        main.startLifetime = landingForce * 0.05f;

        // Set the emission rate based on the landing force
        var emission = _landingParticles.emission;
        emission.rateOverTime = landingForce * 5;

        // Play the particles
        _landingParticles.Stop();
        _landingParticles.Play();
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collided object is on the ground layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Determine the landing force based on the maximum height achieved
            float landingForce = BikeController.Instance.CalculateLandingForce(maxAirHeight, transform.position.y);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
            //Debug.Log("Landing ForcE: "  + landingForce);
            // Play the landing particle effect
            PlayLandingParticles(landingForce);

            // Reset the maximum height
            maxAirHeight = transform.position.y;
        }
    }

}
