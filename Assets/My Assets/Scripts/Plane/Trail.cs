using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] Plane plane;

    // Start is called before the first frame update
    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        trailRenderer.emitting = false;
        if (plane.LocalAngularVelocity.magnitude >= 0.3f)
        {
            trailRenderer.emitting = true;
        }
    }
}
