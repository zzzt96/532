using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup endingGroup;
    public Image displayImage;

    [Header("Settings")]
    public Sprite[] endingImages; // 在这里放入你的2张结尾图片
    public string startSceneName; // 开始场景的名字
    public float fadeDuration = 1.0f;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (endingImages.Length > 0)
        {
            displayImage.sprite = endingImages[0];
            StartCoroutine(Fade(0, 1)); // 初始淡入第一张
        }
    }

    void Update()
    {
        if (!isTransitioning && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            ShowNextEnding();
        }
    }

    void ShowNextEnding()
    {
        currentIndex++;
        if (currentIndex < endingImages.Length)
        {
            StartCoroutine(SwitchImage());
        }
        else
        {
            // 播完了，回到开始场景
            SceneManager.LoadScene(startSceneName);
        }
    }

    IEnumerator SwitchImage()
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(1, 0)); // 淡出当前
        displayImage.sprite = endingImages[currentIndex];
        yield return StartCoroutine(Fade(0, 1)); // 淡入下一张
        isTransitioning = false;
    }

    IEnumerator Fade(float start, float end)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            endingGroup.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        endingGroup.alpha = end;
    }
}