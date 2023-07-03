using System.Collections;
using UnityEngine;

public class BikeParticles : MonoBehaviour
{
    ParticleSystem dirtParticles;
    ParticleSystem landingParticles;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] BikeComponents bikeComponents;


    float speedThreshold = 0.2f;
    float minEmissionRate = 35f;
    float maxEmissionRate = 100f;

    public float minParticleSpeed; 
    public float maxParticleSpeed;

    public float minParticleSize; 
    public float maxParticleSize;

    float maxBikeSpeed;

    RaycastHit2D rearHit;
    RaycastHit2D bikeHit;

    private void Awake()
    {

    }

    private void Start()
    {
        maxBikeSpeed = 10f;
    }

    void Update()
    {
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
