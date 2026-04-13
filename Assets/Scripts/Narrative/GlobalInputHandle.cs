using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalInputHandler : MonoBehaviour
{
    private static GlobalInputHandler instance;

    void Awake()
    {
        // 确保场景切换时这个物体不会被销毁，且全局只有一个
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 1. ESC 退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Game Exiting...");
            Application.Quit();

            // 如果在编辑器模式下，也能看到退出效果
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // 2. Shift + R 重新开始游戏
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Game Restarting...");
            // 加载索引为 0 的场景（通常是你的开始场景）
            // 或者用 SceneManager.LoadScene("你的开始场景名");
            SceneManager.LoadScene(0);
        }
    }
}