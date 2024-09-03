using UnityEngine;

public class GameManage : MonoBehaviour
{
    [SerializeField] public bool gameHasEnded = false;
    [SerializeField] private float restartDelay = 1f;

    [SerializeField] private GameObject completeLevelUI;
    [SerializeField] private GameObject timerUI;
    [SerializeField] private PauseMenu pauseMenu;

    [SerializeField] private GameObject player;
    [SerializeField] public GameObject playerInstance;
    [SerializeField] private GameObject mapCamera;
    [SerializeField] private Transform spawnPoint;
    


    public void SpawnPlayer()
    {
        playerInstance = Instantiate(player, spawnPoint.position, spawnPoint.rotation);
    }

    public void StartLevel() 
    {
        Timer.instance.BeginTimer();
        mapCamera.SetActive(false);
        SpawnPlayer();
    }

    public void KillPlayer()
    {
        Destroy(this.playerInstance);
    }

    public void FinishedLevel()
    {
        completeLevelUI.SetActive(true);
        timerUI.SetActive(false);
        EndGame();
    }

    public void EndGame()
    {
        if (gameHasEnded == false)
        {
            gameHasEnded = true;
        }
    }

    public void Dead()
    {
        Invoke("PauseGame", restartDelay);
    }

    public void PauseGame()
    {
        pauseMenu.Pause();
    }
}
