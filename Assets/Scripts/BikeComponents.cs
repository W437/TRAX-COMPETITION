using UnityEngine;

[System.Serializable]
public class BikeComponents : MonoBehaviour
{
    [SerializeField] WheelJoint2D backWheel;
    [SerializeField] WheelJoint2D frontWheel;
    [SerializeField] Rigidbody2D rb_BackWheel;
    [SerializeField] Rigidbody2D rb_FrontWheel;
    [SerializeField] CircleCollider2D rearWheelCollider;
    [SerializeField] Transform backWheelTransform;
    [SerializeField] Transform frontWheelTransform;
    [SerializeField] CapsuleCollider2D bikeBody;
    [SerializeField] SpriteRenderer bikeBodyRenderer;
    [SerializeField] SpriteRenderer frontWheelRenderer;
    [SerializeField] SpriteRenderer backWheelRenderer;
    [SerializeField] ParticleSystem dirtParticles;
    [SerializeField] ParticleSystem landingParticles;
    [SerializeField] Collider2D groundCheckCollider;
    [SerializeField] WheelJoint2D bikeWheelJoint;
    [SerializeField] JointMotor2D bikeMotor;
    [SerializeField] Rigidbody2D rb_Bike;

    //[SerializeField] TrailRenderer trailRenderer;

    //private TrailManager trailManager; // Declare this

    [SerializeField] float motorSpeed;
    [SerializeField] float downwardForce;
    [SerializeField] float accelerationTime;
    [SerializeField] float groundCheckDistance;
    [SerializeField] float initialMaxTorque; // Starting torque
    [SerializeField] float maxTorque; // Starting torque
    [SerializeField] float maxAirRotationSpeed; // Starting torque
    [SerializeField] float flipTorque;


    [SerializeField] Color originalBikeColor;
    [SerializeField] Color originalFrontWheelColor;
    [SerializeField] Color originalBackWheelColor;
    [SerializeField] Color originalTrailColor;

    /*
    public void SetTrail(int trailId)
    {
        // Get reference to TrailManager
        trailManager = TrailManager.Instance;

        // Get selected trail prefab
        GameObject selectedTrailPrefab = trailManager.GetSelectedTrail(trailId);

        // Instantiate trail and assign it to trailRenderer
        GameObject trailInstance = Instantiate(selectedTrailPrefab, transform.position, Quaternion.identity, transform);
        trailRenderer = trailInstance.GetComponent<TrailRenderer>();
    }

    public TrailRenderer TrailRenderer
    {
        get { return trailRenderer; }
        set { trailRenderer = value; }
    }
    */

    public float MaxAirRotationSpeed
    {
        get { return maxAirRotationSpeed; }
        set { maxAirRotationSpeed = value; }
    }


    public float FlipTorque
    {
        get { return flipTorque; }
        set { flipTorque = value; }
    }

    public Color OriginalBikeColor
    {
        get { return originalBikeColor; }
        set { originalBikeColor = value; }
    }

    public Color OriginalFrontWheelColor
    {
        get { return originalFrontWheelColor; }
        set { originalFrontWheelColor = value; }
    }

    public Color OriginalBackWheelColor
    {
        get { return originalBackWheelColor; }
        set { originalBackWheelColor = value; }
    }

    public Color OriginalTrailColor
    {
        get { return originalTrailColor; }
        set { originalTrailColor = value; }
    }


    public float MaxTorque
    {
        get { return maxTorque; }
        set { maxTorque = value; }
    }

    public float MotorSpeed
    {
        get { return motorSpeed; }
        set { motorSpeed = value; }
    }

    public float DownwardForce
    {
        get { return downwardForce; }
        set { downwardForce = value; }
    }

    public float AccelerationTime
    {
        get { return accelerationTime; }
        set { accelerationTime = value; }
    }

    public float GroundCheckDistance
    {
        get { return groundCheckDistance; }
        set { groundCheckDistance = value; }
    }

    public float InitialMaxTorque
    {
        get { return initialMaxTorque; }
        set { initialMaxTorque = value; }
    }

    public WheelJoint2D BackWheel
    {
        get { return backWheel; }
        set { backWheel = value; }
    }

    public WheelJoint2D FrontWheel
    {
        get { return frontWheel; }
        set { frontWheel = value; }
    }

    public Rigidbody2D RB_BackWheel
    {
        get { return rb_BackWheel; }
        set { rb_BackWheel = value; }
    }

    public Rigidbody2D RB_FrontWheel
    {
        get { return rb_FrontWheel; }
        set { rb_FrontWheel = value; }
    }

    public CircleCollider2D RearWheelCollider
    {
        get { return rearWheelCollider; }
        set { rearWheelCollider = value; }
    }

    public Transform BackWheelTransform
    {
        get { return backWheelTransform; }
        set { backWheelTransform = value; }
    }

    public Transform FrontWheelTransform
    {
        get { return frontWheelTransform; }
        set { frontWheelTransform = value; }
    }

    public CapsuleCollider2D BikeBody
    {
        get { return bikeBody; }
        set { bikeBody = value; }
    }

    public SpriteRenderer BikeBodyRenderer
    {
        get { return bikeBodyRenderer; }
        set { bikeBodyRenderer = value; }
    }

    public SpriteRenderer FrontWheelRenderer
    {
        get { return frontWheelRenderer; }
        set { frontWheelRenderer = value; }
    }

    public SpriteRenderer BackWheelRenderer
    {
        get { return backWheelRenderer; }
        set { backWheelRenderer = value; }
    }


    public ParticleSystem DirtParticles
    {
        get { return dirtParticles; }
        set { dirtParticles = value; }
    }

    public ParticleSystem LandingParticles
    {
        get { return landingParticles; }
        set { landingParticles = value; }
    }

    public Collider2D GroundCheckCollider
    {
        get { return groundCheckCollider; }
        set { groundCheckCollider = value; }
    }

    public WheelJoint2D BikeWheelJoint
    {
        get { return bikeWheelJoint; }
        set { bikeWheelJoint = value; }
    }

    public JointMotor2D BikeMotor
    {
        get { return bikeMotor; }
        set { bikeMotor = value; }
    }

    public Rigidbody2D RB_Bike
    {
        get { return rb_Bike; }
        set { rb_Bike = value; }
    }
}
