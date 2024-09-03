using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI airSpeed;
    [SerializeField] TextMeshProUGUI throttle;
    [SerializeField] TextMeshProUGUI altitude;
    [SerializeField] TextMeshProUGUI climbRate;
    [SerializeField] GameObject stallUI;

    float lastPos = 0;

    public void UpdateAirSpeed(Vector3 velocity)
    {
        airSpeed.text = "Airspeed: " + (velocity.z * 1.944f).ToString("F0") + "Knots";
    }

    public void UpdateAltitude(Vector3 pos)
    { 
        float vPos = pos.y * 3.281f;
        altitude.text = "Altitude: " + vPos.ToString("F0") + " Feet";
        climbRate.text = "Climb Rate: " + ((vPos - lastPos)/Time.fixedDeltaTime).ToString("F0") + "Feet/s";
        lastPos = vPos;
    }

    public void UpdateThrottle(float throttlePercent)
    {
        throttle.text = "Throttle: " + (throttlePercent * 100).ToString("F0") + "%";
    }

    public void UpdateStall(bool stall, bool stallWarning)
    {
        if (stallWarning == true)
        {
            stallUI.SetActive(true);
        }
        else
        {
            stallUI.SetActive(false);
        }
    }
}
