using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    public TextMeshProUGUI timeCounter;

    public TextMeshProUGUI timeFinish;

    private TimeSpan timePlaying;

    private bool timerGoing;

    private float elapsedTime;

    void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        timeCounter.text = "00:00.00";
        timerGoing = false;
    }

    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
    }    

    private IEnumerator UpdateTimer()
    {
        while(timerGoing)
        {
            elapsedTime += Time.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayerStr = timePlaying.ToString("mm':'ss'.'ff");
            timeCounter.text = timePlayerStr;
            timeFinish.text = timePlayerStr;

            yield return null;
        }
    }
}
