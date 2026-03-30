using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Ҫ���صĳ������ƣ���ȷ�������ӵ� Build Settings ��")]
    public string nextSceneName;

    private bool isLoading = false;

    void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;

        bool canTrigger = false;

        // ��鴥�����ǲ�����ң����걾�壩
        if (other.GetComponent<PlayerController>() != null)
        {
            canTrigger = true;
        }
        else
        {
            // �����Ҹ�����������ϣ���鴥�����ǲ��Ǳ����������
            ToyBase toy = other.GetComponent<ToyBase>();
            // ������ other.GetComponentInParent<ToyBase>() ���������㼶����

            // Ϊ�˰�ȫ��ȷ��ֻ����ҵ�ǰ��������߲��ܴ������أ������κ�����������㣬�������ƣ�
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

    // ����һ�������������������Ժ󲻽�����ͨ����ײ������������ͨ����ť�����������������
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