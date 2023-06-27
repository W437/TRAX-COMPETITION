using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public static TrailManager Instance; // singleton instance

    [SerializeField] private Trail[] trails;

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
        foreach (var trail in trails)
        {
            if (trail.trailId == selectedTrailId)
            {
                return trail.trailPrefab;
            }
        }
        return null; // if no match is found
    }

    public Trail[] GetAllTrails()
    {
        return trails;
    }

    public Trail GetTrailById(int id)
    {
        return trails.FirstOrDefault(t => t.trailId == id);
    }

}
