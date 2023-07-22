using UnityEngine;

[CreateAssetMenu(fileName = "TrailData", menuName = "Game/Bike Trail")]
public class Trail : ScriptableObject
{
    public GameObject TrailPrefab;
    public int ID;
    public int PRICE;
}