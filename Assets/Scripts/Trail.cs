using UnityEngine;

[CreateAssetMenu(fileName = "TrailData", menuName = "Game/Bike Trail")]
public class Trail : ScriptableObject
{
    public GameObject trailPrefab;
    public int trailId;
    public int price;
}