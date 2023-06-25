using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BikeData", menuName = "Game/Player Bike")]
public class Bike : ScriptableObject
{
    public int bikeId;
    public int price;
    public GameObject bikePrefab;
    public int maxSpeed;
    public int mass;
    public int angularDrag;
    public TrailRenderer bikeTrailRenderer;
    public BikeComponents bikeComponents;
    public WheelJoint2D BackWheel;
    public WheelJoint2D FrontWheel;
    public Rigidbody2D RB_BackWheel;
    public Rigidbody2D RB_FrontWheel;
    public CircleCollider2D RearWheelCollider;
    public Transform BackWheelTransform;
    public Transform FrontWheelTransform;
    public CapsuleCollider2D BikeBody;
    public SpriteRenderer BikeBodyRenderer;
    public SpriteRenderer FrontWheelRenderer;
    public SpriteRenderer BackWheelRenderer;
    public TrailRenderer BikeTrailRenderer;
    public LayerMask GroundLayer;
    public float MotorSpeed;
    public float MaxTorque;
    public float DownwardForce;
    public float AccelerationTime;
    public float GroundCheckDistance;
    public float InitialMaxTorque;
    public Collider2D GroundCheckCollider;
}
