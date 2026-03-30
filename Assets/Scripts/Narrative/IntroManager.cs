using UnityEngine;
using UnityEngine.UI;
using TMPro; // ʹ�� TextMeshPro
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("�����˳�ʼͼƬ�����壨������� CanvasGroup �����")]
    public CanvasGroup introImageGroup;
    [Tooltip("������ʾ������������")]
    public TextMeshProUGUI storyText;

    [Header("Story Settings")]
    [Tooltip("ͼƬ�����ɺ�����Ҫ��ʱ�䣨�룩")]
    public float fadeDuration = 1.5f;

    [Tooltip("������������ľ������֣�ÿ��һ��Element������ҵ�����һҳ")]
    [TextArea(3, 5)] // �����������������󣬷����������������
    public string[] storyLines;

    [Header("Scene Transition")]
    [Tooltip("���������Ҫ���ص���һ����������")]
    public string nextSceneName;

    private int currentLineIndex = 0;

    // ״̬������ǰ����ʲô�׶�
    private enum IntroState { ShowingImage, FadingImage, ShowingText }
    private IntroState currentState = IntroState.ShowingImage;

    void Start()
    {
        // ��ʼ��״̬����ʾͼƬ����������
        if (introImageGroup != null)
        {
            introImageGroup.alpha = 1f;
            introImageGroup.gameObject.SetActive(true);
        }

        if (storyText != null)
        {
            storyText.text = ""; // �������
            storyText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // �����ҵ�������������߰��¿ո�/�س���
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentState == IntroState.ShowingImage)
            {
                // ��ҵ�һ�ε������ʼ����ͼƬ
                StartCoroutine(FadeOutImageAndStartText());
            }
            else if (currentState == IntroState.ShowingText)
            {
                // ֮��ĵ�����л���һ������
                ShowNextLine();
            }
        }
    }

    IEnumerator FadeOutImageAndStartText()
    {
        currentState = IntroState.FadingImage; // ����״̬����ֹ����ʱ��ҿ��
        float elapsed = 0f;

        // ͼƬ������͸��
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (introImageGroup != null)
            {
                introImageGroup.alpha = 1f - (elapsed / fadeDuration);
            }
            yield return null;
        }

        // �����������
        if (introImageGroup != null)
        {
            introImageGroup.alpha = 0f;
            introImageGroup.gameObject.SetActive(false); // ����ͼƬ��¶�����µĺ���
        }

        // ��΢ͣ�ٰ��루���ף����ݳ�����Ȼ��
        yield return new WaitForSeconds(0.5f);

        // ��������ģʽ����ʾ��һ�仰
        currentState = IntroState.ShowingText;
        if (storyText != null) storyText.gameObject.SetActive(true);
        ShowNextLine();
    }

    void ShowNextLine()
    {
        // �������ʣ�µľ���
        if (currentLineIndex < storyLines.Length)
        {
            if (storyText != null)
            {
                storyText.text = storyLines[currentLineIndex];
            }
            currentLineIndex++; // ׼����һ�ε��������
        }
        else
        {
            // ���о��鲥����ϣ�������Ϸ������
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
            Debug.LogWarning("[IntroManager] ��û����д��һ�����������֣�");
        }
    }
}