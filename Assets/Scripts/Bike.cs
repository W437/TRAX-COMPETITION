using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BikeData", menuName = "Game/Player Bike")]
public class Bike : ScriptableObject
{
    public int id;
    public int price;
    public GameObject bikePrefab;
    public GameObject bikeTrail;
    public int maxSpeed;
    public int motorSpeed;
    public int accelerationTime;
    public int initialMaxTorque;
    public int maxTorque;
    public int mass;
    public int angularDrag;

    // wheels
    public float dampingRatio;
    public float frequency;
}
