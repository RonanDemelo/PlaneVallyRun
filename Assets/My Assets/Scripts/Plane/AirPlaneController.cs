using UnityEngine;
using UnityEngine.InputSystem;
public class AirPlaneController : MonoBehaviour
{

    private PlaneController planeController;

    [SerializeField] Plane plane;

    [SerializeField] Vector3 controlInput;

    [SerializeField] private PlayerInput playerInput;

    public PlayerInput PlayerInput => playerInput;

    private void Awake()
    {
        planeController = new PlaneController();
    }

    private void OnEnable()
    {
        planeController.Enable();
    }

    private void OnDisable()
    {
        planeController.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetPlane(plane);
    }

    void SetPlane(Plane plane)
    {
        this.plane = plane;

    }

    public void Thrust(InputAction.CallbackContext context)
    {
        if (plane == null) return;
        plane.SetThrottleInput(context.ReadValue<float>());

        
    }

    public void OnRollPitchInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;
        var input = context.ReadValue<Vector2>();
        controlInput = new Vector3(input.y, controlInput.y, -input.x);
    }

    public void OnYawInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;
        var input = context.ReadValue<float>();
        controlInput = new Vector3(controlInput.x, input, controlInput.z);
    }

    public void Flaps(InputAction.CallbackContext context)
    {
        if(plane == null) return;
        if (context.phase == InputActionPhase.Performed)
        {
            plane.ToggleFlaps();
        }
    }

    public void AirBreak(InputAction.CallbackContext context)
    {
        if (plane == null) return;
        if (context.phase == InputActionPhase.Performed)
        {
            plane.EneableAirBreak();
        }
    }

    //bool IsAnimationPlaying(Animator animator, string stateName)
    //{
    //    if(animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
    //     animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    //    {
    //        return true;

    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (plane == null) return;

        plane.SetControlInput(controlInput);
    }
}
