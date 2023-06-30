using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public static LevelEnd Instance;
    private bool collided = false;
    public ParticleSystem finishLineParticles;  // Drag your Particle System here in the Inspector

    // Start is called before the first frame update
    void Start()
    {
        
    }


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
            // Perform actions when the player collides with the finish line
            ScreenManager.Instance.OnLevelEnd();
        }
    }

}
