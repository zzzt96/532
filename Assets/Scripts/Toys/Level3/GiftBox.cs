using System.Collections;
using UnityEngine;

public class GiftBox : MonoBehaviour
{
    [Header("Box")]
    public Transform box;
    public Vector3 boxFallRotation = new Vector3(0f, 0f, 90f);
    public Vector3 boxFallSlide = new Vector3(0.3f, 0f, 0f);
    public float boxFallDuration = 0.4f;

    [Header("Gift")]
    public GameObject gift;
    public Vector3 giftSlideWorldPosition;
    public Vector3 giftFinalWorldPosition;
    public float giftSlideDuration = 0.4f;
    public float giftFallDuration = 0.3f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("纸盒打开的纸质摩擦+结构展开声 (盒子被推倒瞬间播放)")]
    public SoundSlot paperOpenSound;
    // ===============================================

    bool triggered = false;
    AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    public void TriggerKnock()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(KnockSequence());
    }

    IEnumerator KnockSequence()
    {
        // 盒子被推倒瞬间: 纸质打开声
        PlayOneShotSlot(paperOpenSound);

        if (box != null)
            yield return StartCoroutine(KnockBox());

        yield return new WaitForSeconds(0.2f);

        if (gift != null)
            yield return StartCoroutine(SlideGift());

        yield return new WaitForSeconds(0.3f);

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

        float elapsed = 0f;
        while (elapsed < giftSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / giftSlideDuration), 3f);
            gift.transform.position = Vector3.Lerp(startPos, giftSlideWorldPosition, t);
            yield return null;
        }
        gift.transform.position = giftSlideWorldPosition;

        elapsed = 0f;
        while (elapsed < giftFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / giftFallDuration);
            float eased = t * t;
            gift.transform.position = Vector3.Lerp(giftSlideWorldPosition, giftFinalWorldPosition, eased);
            yield return null;
        }
        gift.transform.position = giftFinalWorldPosition;
    }

    void PlayOneShotSlot(SoundSlot slot)
    {
        if (slot == null || slot.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.pitch = slot.pitch +
            Random.Range(-slot.randomPitchRange, slot.randomPitchRange);
        audioSrc.PlayOneShot(slot.clip, slot.volume);
    }
}