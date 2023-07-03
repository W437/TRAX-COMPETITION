using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeSound : MonoBehaviour
{
    public static BikeSound Instance;
    public AudioSource audioSource;
    public AudioClip accelerationClip;
    public float minPitch = 0.1f;
    public float maxPitch = 2f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        UpdateSound(BikeController.Instance.GetBikeSpeed());
    }

    public void UpdateSound(float deltaTime)
    {
        // Example usage: UpdateSound(Time.deltaTime);
        float speed = BikeController.Instance.GetBikeSpeed();

        // Normalize the speed to a range of 0 to 1
        float normalizedSpeed = Mathf.InverseLerp(0f, 10, speed);

        // Map the normalized speed to the pitch range
        float pitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        audioSource.pitch = pitch;

        if (!audioSource.isPlaying || audioSource.clip != accelerationClip)
        {
            audioSource.clip = accelerationClip;
            audioSource.Play();
        }
    }


}
