using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAnimator : MonoBehaviour
{
    [SerializeField]
    List<GameObject> afterburnerGraphics;
    [SerializeField]
    float afterburnerThreshold;
    [SerializeField]
    float afterburnerMinSize;
    [SerializeField]
    float afterburnerMaxSize;
    [SerializeField]
    float maxAileronDeflection;
    [SerializeField]
    float maxElevatorDeflection;
    [SerializeField]
    float maxRudderDeflection;
    [SerializeField]
    float airbrakeDeflection;
    [SerializeField]
    float flapsDeflection;
    [SerializeField]
    float deflectionSpeed;
    [SerializeField]
    Transform rightAileron;
    [SerializeField]
    Transform leftAileron;
    [SerializeField]
    List<Transform> elevators;
    [SerializeField]
    List<Transform> rudders;
    [SerializeField]
    List<GameObject> landingGear;
    [SerializeField]
    List<Transform> flaps;
    [SerializeField]
    float maxPropellerRotationSpeed = 1500f;
    [SerializeField]
    float propellerAcceleration = 300f;
    public float currentPropellerSpeed = 0f;
    [SerializeField] bool isPropeller;

    Plane plane;
    List<Transform> engineTransform;
    Dictionary<Transform, Quaternion> neutralPoses;
    Vector3 deflection;
    float airbrakePosition;
    float flapsPosition;

    void Start()
    {
        plane = GetComponent<Plane>();
        engineTransform = new List<Transform>();
        neutralPoses = new Dictionary<Transform, Quaternion>();

        foreach (var go in afterburnerGraphics)
        {
            engineTransform.Add(go.GetComponent<Transform>());
        }

        AddNeutralPose(leftAileron);
        AddNeutralPose(rightAileron);

        foreach (var t in elevators)
        {
            AddNeutralPose(t);
        }

        foreach (var t in rudders)
        {
            AddNeutralPose(t);
        }

        foreach (var t in flaps)
        {
            AddNeutralPose(t);
        }
    }

    void AddNeutralPose(Transform transform)
    {
        neutralPoses.Add(transform, transform.localRotation);
    }

    Quaternion CalculatePose(Transform transform, Quaternion offset)
    {
        return neutralPoses[transform] * offset;
    }

    //Code for afterbuners
    void UpdateAfterburners()
    {
        
    }

    void UpdateEngine()
    {
        if(isPropeller == true)
        {
            maxPropellerRotationSpeed = plane.Throttle * 2000;

            // Gradually increase the currentPropellerSpeed up to the maxPropellerRotationSpeed.
            currentPropellerSpeed = Mathf.MoveTowards(currentPropellerSpeed, maxPropellerRotationSpeed, propellerAcceleration * Time.deltaTime);

            // Calculate the rotation amount based on the currentPropellerSpeed.
            float rotationAmount = currentPropellerSpeed * Time.deltaTime;

            // Rotate the propeller.
            for(int i = 0; i < afterburnerGraphics.Count; i++)
                engineTransform[i].Rotate(Vector3.up * rotationAmount);
        }
        else
        {
            float throttle = plane.Throttle;
            float afterburnerT = Mathf.Clamp01(Mathf.InverseLerp(afterburnerThreshold, 1, throttle));
            float size = Mathf.Lerp(afterburnerMinSize, afterburnerMaxSize, afterburnerT);

            if (throttle >= afterburnerThreshold)
            {
                for (int i = 0; i < afterburnerGraphics.Count; i++)
                {
                    afterburnerGraphics[i].SetActive(true);
                    engineTransform[i].localScale = new Vector3(size, size, size);
                }
            }
            else
            {
                for (int i = 0; i < afterburnerGraphics.Count; i++)
                {
                    afterburnerGraphics[i].SetActive(false);
                }
            }
        }
    }

    void UpdateControlSurfaces(float dt)
    {
        var input = plane.EffectiveInput;

        deflection.x = Utilities.MoveTo(deflection.x, input.x, deflectionSpeed, dt, -1, 1);
        deflection.y = Utilities.MoveTo(deflection.y, input.y, deflectionSpeed, dt, -1, 1);
        deflection.z = Utilities.MoveTo(deflection.z, input.z, deflectionSpeed, dt, -1, 1);

        rightAileron.localRotation = CalculatePose(rightAileron, Quaternion.Euler(0, 0, deflection.z * maxAileronDeflection));
        leftAileron.localRotation = CalculatePose(leftAileron, Quaternion.Euler(0, 0, -deflection.z * maxAileronDeflection));

        foreach (var t in elevators)
        {
            t.localRotation = CalculatePose(t, Quaternion.Euler(0, 0, deflection.x * maxElevatorDeflection));
        }

        foreach (var t in rudders)
        {
            t.localRotation = CalculatePose(t, Quaternion.Euler(0, -deflection.y * maxRudderDeflection, 0));
        }
    }

    void UpdateAirBrakesAndFlaps(float dt)
    {
        var airBrakeTarget = plane.AirBrakeDeployed ? 1 : 0;
        var flapsTarget = plane.FlapsDeployed ? 1 : 0;

        airbrakePosition = Utilities.MoveTo(airbrakePosition, airBrakeTarget, deflectionSpeed, dt);
        flapsPosition = Utilities.MoveTo(flapsPosition, flapsTarget, deflectionSpeed, dt);

        foreach (var t in flaps)
        {
            t.localRotation = CalculatePose(t, (Quaternion.Euler(0, 0, 
                ((airbrakePosition * airbrakeDeflection) + (flapsPosition * flapsDeflection)) ) ));
        }
    }



    void LateUpdate()
    {
        float dt = Time.deltaTime;

        UpdateEngine();
        UpdateControlSurfaces(dt);
        UpdateAirBrakesAndFlaps(dt);
    }
}
