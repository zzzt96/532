using UnityEngine;
using System.Collections;

public class TutorialBall : MonoBehaviour
{
    [Header("References")]
    public TutorialManager tutorialManager;
    public Transform basketTarget;   // 篮子中心的目标点 (Empty GameObject)

    [Header("Animation Settings")]
    public float flyDuration = 0.8f; // 飞进篮子需要几秒
    public float arcHeight = 1.5f;   // 抛物线的高度

    private bool hasBeenHit = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit) return;

        // 只要碰到的物体上有 TutorialToy 脚本，就触发！
        if (other.GetComponent<TutorialToy>() != null)
        {
            hasBeenHit = true;
            StartCoroutine(FlyToBasket());
        }
    }

    IEnumerator FlyToBasket()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = basketTarget.position;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            // 线性移动 X 和 Z
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            // 使用 Sin 函数为 Y 轴添加完美的抛物线高度
            currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = endPos; // 确保精准落入

        // 告诉管理器：球进啦！
        if (tutorialManager != null)
        {
            tutorialManager.OnBallInBasket();
        }
    }
}