using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BikeData", menuName = "Game/Player Bike")]
public class Bike : ScriptableObject
{
    public GameObject bikePrefab;
    public GameObject bikeTrail;
    public int bikeId;
    public int trailId;
    public int price;
    public int maxSpeed;
    public int mass;
}
