using UnityEngine;


[CreateAssetMenu(fileName = "BikeData", menuName = "Game/Player Bike")]
public class Bike : ScriptableObject
{
    public GameObject BikePrefab;
    public int ID;
    public int PRICE;
    public int MaxSpeed;
    public int Mass;
}
