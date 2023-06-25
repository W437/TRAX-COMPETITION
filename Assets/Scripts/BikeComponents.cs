using UnityEngine;

public class BikeComponents : MonoBehaviour
{
    public WheelJoint2D BackWheel;
    public WheelJoint2D FrontWheel;
    public Rigidbody2D RB_BackWheel;
    public Rigidbody2D RB_FrontWheel;
    public Rigidbody2D RB_Bike;
    public CircleCollider2D RearWheelCollider;
    public Transform BackWheelTransform;
    public Transform FrontWheelTransform;
    public CapsuleCollider2D BikeBody;
    public SpriteRenderer BikeBodyRenderer;
    public SpriteRenderer FrontWheelRenderer;
    public SpriteRenderer BackWheelRenderer;
    public TrailRenderer BikeTrailRenderer;
}
