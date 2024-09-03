using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAudio : MonoBehaviour
{
    [SerializeField] Plane plane;
    [SerializeField] PlaneAnimator animator;
    //[SerializeField] PauseMenu pauseMenu;

    [SerializeField] public AudioSource planeEngine;

    // Start is called before the first frame update
    void Start()
    {
        planeEngine.Play();
    }

    // Update is called once per frame
    void Update()
    {
        planeEngine.pitch = (animator.currentPropellerSpeed / 2000) + 0.3f;

        Dead();
        //if(pauseMenu.GameIsPaused == true) 
        //{
        //    planeEngine.Pause();
        //}
        //else
        //{
        //    planeEngine.UnPause();
        //}
    }

    private void Dead()
    {
        if(plane.Dead == true)
        {
            planeEngine.Pause();
        }
    }
}
