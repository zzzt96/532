using UnityEngine;
using UnityEngine.UI;
using TMPro; // 使用 TextMeshPro
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("挂载了初始图片的物体（必须包含 CanvasGroup 组件）")]
    public CanvasGroup introImageGroup;
    [Tooltip("用来显示剧情的文字组件")]
    public TextMeshProUGUI storyText;

    [Header("Story Settings")]
    [Tooltip("图片淡出成黑屏需要的时间（秒）")]
    public float fadeDuration = 1.5f;

    [Tooltip("在这里添加你的剧情文字，每加一个Element就是玩家点击后的一页")]
    [TextArea(3, 5)] // 这个属性能让输入框变大，方便你输入多行文字
    public string[] storyLines;

    [Header("Scene Transition")]
    [Tooltip("剧情结束后要加载的下一个场景名字")]
    public string nextSceneName;

    private int currentLineIndex = 0;

    // 状态机：当前处于什么阶段
    private enum IntroState { ShowingImage, FadingImage, ShowingText }
    private IntroState currentState = IntroState.ShowingImage;

    void Start()
    {
        // 初始化状态：显示图片，隐藏文字
        if (introImageGroup != null)
        {
            introImageGroup.alpha = 1f;
            introImageGroup.gameObject.SetActive(true);
        }

        if (storyText != null)
        {
            storyText.text = ""; // 清空文字
            storyText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 检测玩家点击鼠标左键，或者按下空格/回车键
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentState == IntroState.ShowingImage)
            {
                // 玩家第一次点击，开始淡出图片
                StartCoroutine(FadeOutImageAndStartText());
            }
            else if (currentState == IntroState.ShowingText)
            {
                // 之后的点击，切换下一条文字
                ShowNextLine();
            }
        }
    }

    IEnumerator FadeOutImageAndStartText()
    {
        currentState = IntroState.FadingImage; // 锁定状态，防止淡出时玩家狂点
        float elapsed = 0f;

        // 图片渐渐变透明
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (introImageGroup != null)
            {
                introImageGroup.alpha = 1f - (elapsed / fadeDuration);
            }
            yield return null;
        }

        // 淡出彻底完成
        if (introImageGroup != null)
        {
            introImageGroup.alpha = 0f;
            introImageGroup.gameObject.SetActive(false); // 隐藏图片，露出底下的黑屏
        }

        // 稍微停顿半秒（留白，让演出更自然）
        yield return new WaitForSeconds(0.5f);

        // 进入文字模式，显示第一句话
        currentState = IntroState.ShowingText;
        if (storyText != null) storyText.gameObject.SetActive(true);
        ShowNextLine();
    }

    void ShowNextLine()
    {
        // 如果还有剩下的剧情
        if (currentLineIndex < storyLines.Length)
        {
            if (storyText != null)
            {
                storyText.text = storyLines[currentLineIndex];
            }
            currentLineIndex++; // 准备下一次点击的索引
        }
        else
        {
            // 所有剧情播放完毕，加载游戏场景！
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[IntroManager] Loading next scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[IntroManager] 还没有填写下一个场景的名字！");
        }
    }
}