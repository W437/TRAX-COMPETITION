using System.Collections.Generic;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public static TrailManager Instance; // singleton instance

    public List<Trail> allTrails; // Populate this list in the Unity Editor

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public GameObject GetSelectedTrail(int selectedTrailId)
    {
        // Find the Trail in the allTrails list with the matching ID and return its prefab
        foreach (var trail in allTrails)
        {
            if (trail.trailId == selectedTrailId)
            {
                return trail.trailPrefab;
            }
        }
        return null; // if no match is found
    }
}
