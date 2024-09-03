using UnityEngine;

public class EndTrigger : MonoBehaviour
{

    void OnTriggerEnter()
    {
        FindObjectOfType<GameManage>().FinishedLevel();
        Time.timeScale = 0f;
    }
}
