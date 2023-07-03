using UnityEngine;


[CreateAssetMenu(fileName = "BikeData", menuName = "Game/Player Bike")]
public class Bike : ScriptableObject
{
    public GameObject bikePrefab;
    public int bikeId;
    public int price;
    public int maxSpeed;
    public int mass;
}
