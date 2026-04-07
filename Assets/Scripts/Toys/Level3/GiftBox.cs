using System.Collections;
using UnityEngine;

/// <summary>
/// 礼物盒子
/// 小猫跳上桌后自动触发：
/// 1. 盒子被推倒动画
/// 2. 礼物从盒子里滑出
/// 3. 通知Manager小女孩走过来
/// </summary>
public class GiftBox : MonoBehaviour
{
    [Header("Box")]
    [Tooltip("纸盒子Transform")]
    public Transform box;
    [Tooltip("盒子推倒的目标世界旋转")]
    public Vector3 boxFallRotation = new Vector3(0f, 0f, 90f);
    [Tooltip("盒子推倒时的位移")]
    public Vector3 boxFallSlide = new Vector3(0.3f, 0f, 0f);
    public float boxFallDuration = 0.4f;

    [Header("Gift")]
    [Tooltip("礼物GameObject（初始隐藏在盒子里）")]
    public GameObject gift;
    [Tooltip("礼物滑出的目标世界位置")]
    public Vector3 giftFinalWorldPosition;
    [Tooltip("礼物滑出动画时间")]
    public float giftSlideDuration = 0.5f;

    bool triggered = false;

    public void TriggerKnock()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(KnockSequence());
    }

    IEnumerator KnockSequence()
    {
        // 1. 盒子被推倒
        if (box != null)
            yield return StartCoroutine(KnockBox());

        yield return new WaitForSeconds(0.2f);

        // 2. 礼物滑出
        if (gift != null)
            yield return StartCoroutine(SlideGift());

        yield return new WaitForSeconds(0.3f);

        // 3. 通知Manager
        Debug.Log("[GiftBox] Gift revealed!");
        Level3Manager.Instance?.OnGiftRevealed();
    }

    IEnumerator KnockBox()
    {
        Quaternion startRot = box.rotation;
        Vector3 startPos = box.position;
        Quaternion endRot = Quaternion.Euler(boxFallRotation);
        Vector3 endPos = startPos + boxFallSlide;

        float elapsed = 0f;
        while (elapsed < boxFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / boxFallDuration), 3f);
            box.rotation = Quaternion.Slerp(startRot, endRot, t);
            box.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        box.rotation = endRot;
        box.position = endPos;
    }

    IEnumerator SlideGift()
    {
        gift.SetActive(true);
        Vector3 startPos = gift.transform.position;
        Vector3 endPos = giftFinalWorldPosition;

        float elapsed = 0f;
        while (elapsed < giftSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / giftSlideDuration), 3f);
            gift.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        gift.transform.position = endPos;
    }
}