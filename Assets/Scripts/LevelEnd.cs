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


}
