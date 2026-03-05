using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("要加载的场景名称，请确保已添加到 Build Settings 中")]
    public string nextSceneName;

    private bool isLoading = false;

    void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;

        bool canTrigger = false;

        // 检查触发的是不是玩家（鬼魂本体）
        if (other.GetComponent<PlayerController>() != null)
        {
            canTrigger = true;
        }
        else
        {
            // 如果玩家附身在玩具身上，检查触发的是不是被附身的玩具
            ToyBase toy = other.GetComponent<ToyBase>();
            // 或者用 other.GetComponentInParent<ToyBase>() 视你的物体层级而定

            // 为了安全，确保只有玩家当前附身的玩具才能触发过关（或者任何玩具碰到都算，看你的设计）
            if (toy != null)
            {
                canTrigger = true;
            }
        }

        if (canTrigger)
        {
            LoadScene();
        }
    }

    // 开放一个公共方法，这样你以后不仅可以通过碰撞触发，还可以通过按钮点击或代码调用来加载
    public void LoadScene()
    {
        if (isLoading) return;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            isLoading = true;
            Debug.Log($"[SceneTransition] Loading next scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[SceneTransition] No scene name provided in the inspector!");
        }
    }
}