using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Start Screen Settings")]
    [Tooltip("开始界面的父物体（需挂载CanvasGroup）")]
    public CanvasGroup startScreenGroup;

    [Header("Comic UI References")]
    [Tooltip("漫画内容组（需挂载CanvasGroup）")]
    public CanvasGroup mainContentGroup;

    [Tooltip("显示漫画图片的组件")]
    public Image comicImageDisplay;

    [Tooltip("显示故事文字的组件")]
    public TextMeshProUGUI storyText;

    [Header("Story Settings")]
    [Tooltip("淡入淡出速度")]
    public float fadeDuration = 1.0f;
    [Tooltip("漫画镜头微动速度")]
    public float zoomSpeed = 0.02f;

    public string[] storyLines;
    public Sprite[] storyImages;

    [Header("Scene Transition")]
    public string nextSceneName;

    private int currentLineIndex = -1;
    private bool isTransitioning = false;
    private bool gameStarted = false; // 标记是否已经点击过 Start

    void Start()
    {
        // 初始状态：显示开始界面，隐藏漫画内容
        if (startScreenGroup != null)
        {
            startScreenGroup.alpha = 1f;
            startScreenGroup.gameObject.SetActive(true);
        }

        if (mainContentGroup != null)
        {
            mainContentGroup.alpha = 0f;
            mainContentGroup.gameObject.SetActive(false);
        }
    }

    // --- 新增：给开始按钮调用的方法 ---
    public void StartGame()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionFromStartToComic());
    }

    IEnumerator TransitionFromStartToComic()
    {
        isTransitioning = true;

        // 1. 淡出开始界面
        yield return StartCoroutine(FadeCanvasGroup(startScreenGroup, 1f, 0f, fadeDuration));
        startScreenGroup.gameObject.SetActive(false);

        // 2. 激活并淡入漫画界面
        mainContentGroup.gameObject.SetActive(true);
        gameStarted = true;
        ShowNextPage(); // 显示第一页

        isTransitioning = false;
    }

    void Update()
    {
        // 只有在游戏开始后，才响应点击翻页
        if (gameStarted && !isTransitioning)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                ShowNextPage();
            }
        }

        // 漫画呼吸感特效
        if (gameStarted && !isTransitioning && comicImageDisplay != null && mainContentGroup.alpha > 0.5f)
        {
            comicImageDisplay.transform.localScale += Vector3.one * zoomSpeed * Time.deltaTime;
        }
    }

    void ShowNextPage()
    {
        currentLineIndex++;
        if (currentLineIndex < storyLines.Length)
        {
            StartCoroutine(TransitionPageCoroutine());
        }
        else
        {
            LoadNextScene();
        }
    }

    IEnumerator TransitionPageCoroutine()
    {
        isTransitioning = true;

        if (currentLineIndex > 0)
        {
            yield return StartCoroutine(FadeCanvasGroup(mainContentGroup, 1f, 0f, fadeDuration / 2f));
        }

        if (storyText != null) storyText.text = storyLines[currentLineIndex];
        if (comicImageDisplay != null && currentLineIndex < storyImages.Length)
        {
            comicImageDisplay.sprite = storyImages[currentLineIndex];
            comicImageDisplay.transform.localScale = Vector3.one;
        }

        yield return StartCoroutine(FadeCanvasGroup(mainContentGroup, 0f, 1f, fadeDuration / 2f));
        isTransitioning = false;
    }

    // 优化的通用渐变方法，支持传入不同的 CanvasGroup
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (cg != null)
                cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        if (cg != null) cg.alpha = endAlpha;
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}