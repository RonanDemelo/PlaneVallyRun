using System.Collections.Generic;
using UnityEngine;

//This code is based on the code and game made by Varzgtiz

public class Plane : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField]
    HUD hud;

    [Header("Thrust")]
    [SerializeField]
    float maxThrust;
    [SerializeField]
    public float throttleSpeed;
    float throttleInput;

    [Header("Lift")]
    [SerializeField]
    float liftPower;
    [SerializeField]
    AnimationCurve liftAOACurve;
    [SerializeField]
    float inducedDrag;
    [SerializeField]
    AnimationCurve inducedDragCurve;
    [SerializeField]
    float rudderPower;
    [SerializeField]
    AnimationCurve rudderAOACurve;
    [SerializeField]
    AnimationCurve rudderInducedDragCurve;
    [SerializeField]
    float flapsLiftPower;
    [SerializeField]
    float flapsAOABias;
    [SerializeField]
    float flapsDrag;
    [SerializeField]
    float flapsRetractSpeed;
    [SerializeField]
    bool stall = false;
    bool stallWarning = false;

    [Header("Drag")]
    [SerializeField]
    AnimationCurve dragForward;
    [SerializeField]
    AnimationCurve dragBack;
    [SerializeField]
    AnimationCurve dragLeft;
    [SerializeField]
    AnimationCurve dragRight;
    [SerializeField]
    AnimationCurve dragTop;
    [SerializeField]
    AnimationCurve dragBottom;
    [SerializeField]
    Vector3 angularDrag;
    [SerializeField]
    float airbrakeDrag;

    [Header("Steering")]
    [SerializeField]
    Vector3 turnSpeed;
    [SerializeField]
    Vector3 turnAcceleration;
    [SerializeField]
    AnimationCurve steeringCurve;
    Vector3 controlInput;

    [Header("Other")]
    [SerializeField]
    bool deployableLandGear;
    [SerializeField]
    List<Collider> landingGear;
    [SerializeField]
    PhysicMaterial landingGearDefaultMaterial;
    [SerializeField]
    PhysicMaterial landingGearBreaksMaterial;
    float initialSpeed;
    [SerializeField]
    bool flapsDeployed;
    [SerializeField]
    bool airBreakDeployed;
    [SerializeField]
    float maxHealth;
    [SerializeField]
    float health;
    [SerializeField]
    GameObject deathEffect;

    float r;

    [Header("Graphics")]
    [SerializeField]
    List<GameObject> graphics;
    //==================================================================================================================================
    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
        set
        {
            maxHealth = Mathf.Max(0, value);
        }
    }

    public float Health
    {
        get
        {
            return health;
        }
        private set
        {
            health = Mathf.Clamp(value, 0, maxHealth);

            if (health == 0 && MaxHealth != 0 && !Dead)
            {
                Die();
            }
        }
    }
    public bool Dead { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public float Throttle { get; private set; }
    public Vector3 EffectiveInput { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirBrakeDeployed {
        get
        {
            return airBreakDeployed;
        }
        private set
        {
            airBreakDeployed = value;
        }
    }
    public bool FlapsDeployed
    {
        get
        {
            return flapsDeployed;
        }
        private set
        {
            flapsDeployed = value;

            foreach (var lg in landingGear)
            {
                if (deployableLandGear == true)
                {
                    lg.enabled = value;
                }
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();


        if (landingGear.Count > 0)
        {
            landingGearDefaultMaterial = landingGear[0].sharedMaterial;
        }

        Rigidbody.velocity = Rigidbody.rotation * new Vector3(0, 0, initialSpeed);
    }

    //Health Code
    public void ApplyDamage(float damage)
    {
        Health -= damage;
    }

    void Die()
    {
        throttleInput = 0;
        Throttle = 0;
        stall = false;
        stallWarning = false;
        Dead = true;

        deathEffect.SetActive(true);
    }

    //Thrust Code
    //get the input of the thrust and sets it to a percentage of what it should be
    public void SetThrottleInput(float input)
    {
        if (Dead) return;
        throttleInput = input;
    }

 

    void UpdateThrottle(float dt)
    {
        //makes sure the thrust can only go upto 100%
        float target = 0;
        if (throttleInput > 0)
        {
            target = 1;
        }

        //calculated the amount of throttle that needs to be applied
        Throttle = Utilities.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);

    }

    //Applies the thrust to the plane causing it to move in its forward direction
    void UpdateThrust()
    {
        Rigidbody.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
    }


    //Drag Code
    void UpdateDrag()
    {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude; // velocity squared


        float airbrakeDrag = AirBrakeDeployed ? this.airbrakeDrag : 0;//gets the drag for the airbreak
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0;//gets the drag for the flaps

        //calculate coefficient of drag depending on direction on velocity
        var coefficient = Utilities.Scale6(
            lv.normalized,
            dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)),
            dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)),
            dragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag + flapsDrag,   //include extra drag for forward coefficient when the flaps or airbreak are deployed
            dragBack.Evaluate(Mathf.Abs(lv.z))
        );

        //Drag = 1/2* velocity^2 *Coefficient of drag
        var drag = coefficient.magnitude * lv2 * -lv.normalized; // drag is opposite direction of velocity

        Rigidbody.AddRelativeForce(drag);

    }

    //calculated angular drag which is how fast an spinning object should come to rest.
    void UpdateAngularDrag()
    {
        var av = LocalAngularVelocity;
        var drag = av.sqrMagnitude * -av.normalized;    //squared, opposite direction of angular velocity
        Rigidbody.AddRelativeTorque(Vector3.Scale(drag, angularDrag), ForceMode.Acceleration);  //ignore rigidbody mass
    }


    //Lift Code

    //calculates the lift on the plane
    Vector3 CalculateLift(float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);    //project velocity onto YZ plane
        var v2 = liftVelocity.sqrMagnitude;                                     //square of velocity

        //lift = velocity^2 * coefficient * liftPower
        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg); //coefficient varies with AOA
        var liftForce = v2 * liftCoefficient * liftPower;

        //lift is perpendicular to velocity
        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        //induced drag varies with square of lift coefficient which acts opposite the airflow
        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce * this.inducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));

        return lift + inducedDrag;
    }

    //applies the lift forces to the plane
    void UpdateLift()
    {
        //only start once the plane is in motion
        if (LocalVelocity.sqrMagnitude < 1f) return;

        //flaps generate extrs lift and increase the AOA of th plane.
        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;

        //the the amount of lift that needs to be generated
        var liftForce = CalculateLift(
            AngleOfAttack + (flapsAOABias * Mathf.Deg2Rad), Vector3.right,
            liftPower + flapsLiftPower,
            liftAOACurve,
            inducedDragCurve
        );

        //stall code
        Stall(liftForce);

        //calculated the lift force in the yaw axis to allow the plane to turn
        var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderInducedDragCurve);

        Rigidbody.AddRelativeForce(liftForce);
        Rigidbody.AddRelativeForce(yawForce);
    }

    void Stall(Vector3 liftForce)
    {
        stall = false;
        stallWarning = false;

        if ((AngleOfAttack * Mathf.Rad2Deg) > 30 && liftForce.y > 1f)
        {
            stall = true;
        }
        if ((AngleOfAttack * Mathf.Rad2Deg) > 25 && liftForce.y > 1f)
        {
            stallWarning = true;
        }
    }

    //calculated the AOA
    void CalculateAngleOfAttack()
    {
        //only if the plane is moving
        if (LocalVelocity.sqrMagnitude < 0.1f)
        {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);//Aoa of pitch axis
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z); // Aoa of Yaw axis
    }

    //State Code
    //this gete the properties of the planes local frame of referance
    void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        Velocity = Rigidbody.velocity;
        LocalVelocity = invRotation * Velocity; // transforms worlds velocity into local space
        LocalAngularVelocity = invRotation * Rigidbody.angularVelocity; // transform into local space

        CalculateAngleOfAttack();
    }


    //Steering Code

    //clamps the inputs to 1 not allowing for excessive inputs to br registered.
    public void SetControlInput(Vector3 input)
    {
        if (Dead) return;
        controlInput = Vector3.ClampMagnitude(input, 1);
    }

    //calculated the torque needed to turn the plane in a single axis
    float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration)
    {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }

    // updated the steering of the plane
    void UpdateSteering(float dt)
    {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed); // gets the steering power needed from the animation curve depending on the speed at which the plane is moving at
        var targetAV = Vector3.Scale(controlInput, turnSpeed * steeringPower); // target angular the plane needs to reach
        var av = LocalAngularVelocity * Mathf.Rad2Deg; //the angualr velocity of the plane

        var correction = new Vector3(
        CalculateSteering(dt, av.x, targetAV.x, turnAcceleration.x * steeringPower),
        CalculateSteering(dt, av.y, targetAV.y, turnAcceleration.y * steeringPower),
        CalculateSteering(dt, av.z, targetAV.z, turnAcceleration.z * steeringPower)
        ); //put the inputs into the calculate steering input to get the torque required to turn the plane.

        Rigidbody.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);//ignore rigidbody mass

        var correctionInput = new Vector3(
            Mathf.Clamp((targetAV.x - av.x) / turnAcceleration.x, -1, 1),
            Mathf.Clamp((targetAV.y - av.y) / turnAcceleration.y, -1, 1),
            Mathf.Clamp((targetAV.z - av.z) / turnAcceleration.z, -1, 1)
        );

        var effectiveInput = (correctionInput + controlInput);

        EffectiveInput = new Vector3(
            Mathf.Clamp(effectiveInput.x, -1, 1),
            Mathf.Clamp(effectiveInput.y, -1, 1),
            Mathf.Clamp(effectiveInput.z, -1, 1)
        );

    }


    //Flaps Code
    //Controls for the flaps
    public void ToggleFlaps()
    {
        if(LocalVelocity.z < flapsRetractSpeed)
        {
            FlapsDeployed = !FlapsDeployed;
        }
    }

    //only deployes flaps if the speed is lower than the flap retract speed.
    void UpdateFlap()
    {
        if (LocalVelocity.z > flapsRetractSpeed)
        {
            FlapsDeployed = false;
        }
    }

    //AirBreak Code
    public void EneableAirBreak()
    {
        AirBrakeDeployed = !AirBrakeDeployed;

        //changed the landing gear mat to a more friction mat to stop the plane when the wheels are making contact with a surface.
        if (AirBrakeDeployed)
        {
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearBreaksMaterial;
            }
        }
        else
        {
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearDefaultMaterial;
            }
        }
    }

    //HUD Code
    private void UpdateHUD()
    {
        hud.UpdateAirSpeed(LocalVelocity);
        hud.UpdateAltitude(transform.position);
        hud.UpdateThrottle(Throttle);
        hud.UpdateStall(stall, stallWarning);
    }


    //Update
    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        CalculateState(dt);
        UpdateFlap();

        UpdateThrottle(dt);

        UpdateThrust();
        UpdateLift();

        UpdateSteering(dt);

        UpdateDrag();
        UpdateAngularDrag();

        CalculateState(dt);

        UpdateHUD();

    }

    void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.contacts[i];

            if (landingGear.Contains(contact.thisCollider))
            {
                return;
            }

            Health = 0;

            Rigidbody.isKinematic = true;
            Rigidbody.position = contact.point;
            Rigidbody.rotation = Quaternion.Euler(0, Rigidbody.rotation.eulerAngles.y, 0);

            foreach (var go in graphics)
            {
                go.SetActive(false);
            }

            

            FindObjectOfType<GameManage>().Dead();

            return;
        }
    }
}
