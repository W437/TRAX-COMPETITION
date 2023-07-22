using System.Linq;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public static TrailManager Instance;

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

    public void ChangeTrailAlpha(GameObject trailPrefab, float alpha)
    {
        // Find the trail prefab's renderer component
        var trailRenderer = trailPrefab.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            // Get the current material
            var trailMaterial = trailRenderer.material;

            // Update the alpha value of the material's color
            var currentColor = trailMaterial.GetColor("_Color");
            currentColor.a = alpha;
            trailMaterial.SetColor("_Color", currentColor);
        }
    }


    public GameObject GetSelectedTrail(int selectedTrailId)
    {
        // Find the Trail in the allTrails list with the matching ID and return its prefab
        foreach (var trail in trails)
        {
            if (trail.ID == selectedTrailId)
            {
                return trail.TrailPrefab;
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
        return trails.FirstOrDefault(t => t.ID == id);
    }

}
