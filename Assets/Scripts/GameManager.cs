using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isIntroPlaying = false;
    public bool girlHasWokenUp = false;

    [Header("References")]
    public LittleGirlControllerWakeUp littleGirl;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GameWin()
    {
        isGameOver = true;
        Debug.Log("Game Win!");
    }

    public void GameLose()
    {
        isGameOver = true;
        Debug.Log("Game Lose!");
    }

    public void WakeUpGirl()
    {
        if (girlHasWokenUp) return;

        girlHasWokenUp = true;
        Debug.Log("[GameManager] Girl is waking up!");

        if (littleGirl != null)
        {
            littleGirl.WakeUpAndMove();
        }
    }

    public void GameComplete()
    {
        isGameOver = true;
        Debug.Log("[GameManager] Game Complete!");
    }
}