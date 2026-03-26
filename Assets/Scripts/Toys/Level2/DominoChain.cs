using UnityEngine;
using System.Collections;

/// <summary>
/// 多米诺链 - 厕纸落地后触发连锁倒书动画
/// 书本依次倒下，最后一本碰到摇椅，摇椅开始晃动
/// 整个过程是脚本动画，不依赖物理
/// </summary>
public class DominoChain : MonoBehaviour
{
    [Header("Books")]
    [Tooltip("按顺序填入所有要倒下的书，从厕纸最近到最远")]
    public Transform[] books;
    [Tooltip("每本书倒下的旋转角度（绕Z轴）")]
    public float bookFallAngle = 85f;
    [Tooltip("每本书倒下的动画时长")]
    public float bookFallDuration = 0.3f;
    [Tooltip("上一本倒下后多久触发下一本")]
    public float chainDelay = 0.15f;

    [Header("Rocking Chair")]
    public RockingChair rockingChair;
    [Tooltip("最后一本书倒下后多久触发摇椅")]
    public float chairDelay = 0.3f;

    private bool isPlaying = false;

    public void StartChain()
    {
        if (isPlaying) return;
        isPlaying = true;
        StartCoroutine(PlayChain());
    }

    IEnumerator PlayChain()
    {
        Debug.Log("[Domino] Chain started!");

        if (books == null || books.Length == 0)
        {
            Debug.LogWarning("[Domino] No books assigned!");
            yield return new WaitForSeconds(chairDelay);
            TriggerChair();
            yield break;
        }

        foreach (Transform book in books)
        {
            if (book == null) continue;
            yield return StartCoroutine(TipBook(book));
            yield return new WaitForSeconds(chainDelay);
        }

        yield return new WaitForSeconds(chairDelay);
        TriggerChair();
    }

    IEnumerator TipBook(Transform book)
    {
        Quaternion startRot = book.localRotation;
        // X轴旋转 = 书向前倒（朝镜头方向倒下）
        // 正值=向前倒，负值=向后倒，根据测试调整
        Quaternion endRot = startRot * Quaternion.Euler(bookFallAngle, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < bookFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / bookFallDuration);
            book.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
        book.localRotation = endRot;
    }
    
    void TriggerChair()
    {
        Debug.Log("[Domino] Last book hit rocking chair!");
        if (rockingChair != null)
            rockingChair.StartRocking();
        else
            Level2Manager.Instance?.OnFanBlowTriggeredChair();
    }
}