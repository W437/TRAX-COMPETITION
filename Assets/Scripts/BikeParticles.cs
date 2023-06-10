using UnityEngine;

public class BikeParticles : MonoBehaviour
{
    public ParticleSystem dirtParticles;
    public Rigidbody2D bikeRigidbody;
    public Transform rearWheel; // The transform of the bike's rear wheel.
    public LayerMask groundLayer; // The LayerMask of the ground.
    public float speedThreshold = 5f;
    public float maxEmissionRate = 50f;
    public float minParticleSpeed = 20f; // The minimum start speed of the particles.
    public float maxParticleSpeed = 50f; // The maximum start speed of the particles.

    void Update()
    {
        // Cast a ray downwards from the rear wheel to find the ground.
        RaycastHit2D hit = Physics2D.Raycast(rearWheel.position, -Vector2.up, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            // If the ray hit the ground, update the position of the Particle System to the hit point.
            dirtParticles.transform.position = hit.point;
        }

        float speed = bikeRigidbody.velocity.magnitude;

        if (speed > speedThreshold && !IsInAir())
        {
            var emission = dirtParticles.emission;
            emission.rateOverTime = (speed / speedThreshold) * maxEmissionRate;

            var main = dirtParticles.main;
            main.startSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, speed / speedThreshold);
        }
        else
        {
            var emission = dirtParticles.emission;
            emission.rateOverTime = 0;

            var main = dirtParticles.main;
            main.startSpeed = minParticleSpeed;
        }
    }

    bool IsInAir()
    {
        return false;
        // Implement this method to check if the bike is in the air.
    }
}
