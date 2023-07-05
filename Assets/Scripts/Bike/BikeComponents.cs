using UnityEngine;

[System.Serializable]
public class BikeComponents : MonoBehaviour
{
    private float bikeColorAlpha;
    private Color originalBikeColor;
    [SerializeField] private WheelJoint2D backWheel;
    [SerializeField] private WheelJoint2D frontWheel;
    [SerializeField] private Rigidbody2D rb_BackWheel;
    [SerializeField] private Rigidbody2D rb_FrontWheel;
    [SerializeField] private CircleCollider2D rearWheelCollider;
    [SerializeField] private Transform backWheelTransform;
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private CapsuleCollider2D bikeBody;
    [SerializeField] private SpriteRenderer bikeBodyRenderer;
    [SerializeField] private SpriteRenderer frontWheelRenderer;
    [SerializeField] private SpriteRenderer backWheelRenderer;
    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private ParticleSystem landingParticles;
    [SerializeField] private Collider2D groundCheckCollider;
    [SerializeField] private WheelJoint2D bikeWheelJoint;
    [SerializeField] private JointMotor2D bikeMotor;
    [SerializeField] private Rigidbody2D rb_Bike;
    [SerializeField] private float motorSpeed;
    [SerializeField] private float downwardForce;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float initialMaxTorque; 
    [SerializeField] private float maxTorque; 
    [SerializeField] private float maxAirRotationSpeed;
    [SerializeField] private float flipTorque;
    [SerializeField] private Color originalFrontWheelColor;
    [SerializeField] private Color originalBackWheelColor;


    public float BikeColorAlpha
    {
        get { return BikeColorAlpha; }
        set
        {
            BikeColorAlpha = value;
            Color bikeColor = GetBikeBodyColor();
            bikeColor.a = BikeColorAlpha; 
            SetBikeBodyColor(bikeColor); 
        }
    }

    public Color OriginalBikeColor
    {
        get { return originalBikeColor; }
        set
        {
            originalBikeColor = value;
            Color bikeColor = GetBikeBodyColor();
            bikeColor.a = originalBikeColor.a; 
            SetBikeBodyColor(bikeColor); 
        }
    }

    private Color GetBikeBodyColor()
    {
        if (bikeBodyRenderer == null)
            bikeBodyRenderer = GetComponent<SpriteRenderer>();

        return bikeBodyRenderer.color;
    }

    private void SetBikeBodyColor(Color color)
    {
        if (bikeBodyRenderer == null)
            bikeBodyRenderer = GetComponent<SpriteRenderer>();

        bikeBodyRenderer.color = color;
    }

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
