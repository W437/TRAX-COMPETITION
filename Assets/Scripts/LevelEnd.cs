using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public static LevelEnd Instance;
    private bool collided = false;
    public ParticleSystem FinishLineParticles;  // Drag your Particle System here in the Inspector


    private void Awake()
    {
        Instance = this;
    }
    
     void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !collided)
        {
            Debug.Log("Finish Collision");
            collided = true;
            FinishLineParticles.Play();

            ScreenManager.Instance.OnLevelFinish();
        }
    }

}
