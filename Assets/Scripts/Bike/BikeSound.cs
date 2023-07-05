using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeSound : MonoBehaviour
{
    public static BikeSound Instance;
    public AudioSource AudioSource;
    public AudioClip AccelerationClip;
    public float MinPitch = 0.1f;
    public float MaxPitch = 2f;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        //UpdateSound(BikeController.Instance.GetBikeSpeed());
    }

    public void UpdateSound(float deltaTime)
    {
        // Example usage: UpdateSound(Time.deltaTime);
        float speed = BikeController.Instance.GetBikeSpeed();

        // Normalize the speed to a range of 0 to 1
        float normalizedSpeed = Mathf.InverseLerp(0f, 10, speed);

        // Map the normalized speed to the pitch range
        float pitch = Mathf.Lerp(MinPitch, MaxPitch, normalizedSpeed);
        AudioSource.pitch = pitch;

        if (!AudioSource.isPlaying || AudioSource.clip != AccelerationClip)
        {
            AudioSource.clip = AccelerationClip;
            AudioSource.Play();
        }
    }


}
