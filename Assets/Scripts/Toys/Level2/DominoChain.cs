using UnityEngine;
using System.Collections;

public class DominoChain : MonoBehaviour
{
    [Header("Books")]
    public Transform[] books;
    public float bookFallAngle = 85f;
    public float bookFallDuration = 0.3f;
    public float chainDelay = 0.15f;

    [Header("Rocking Chair")]
    public RockingChair rockingChair;
    public float chairDelay = 0.3f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("书本倒下的闷响砰+纸张摩擦声 (每本书倒下都触发一次)")]
    public SoundSlot bookFallSound;
    // ===============================================

    private bool isPlaying = false;
    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

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

            // 每本书开始倒下时播一次音 (随机音高让重复听起来不单调)
            PlayOneShotSlot(bookFallSound);

            yield return StartCoroutine(TipBook(book));
            yield return new WaitForSeconds(chainDelay);
        }

        yield return new WaitForSeconds(chairDelay);
        TriggerChair();
    }

    IEnumerator TipBook(Transform book)
    {
        Quaternion startRot = book.localRotation;
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

    void PlayOneShotSlot(SoundSlot slot)
    {
        if (slot == null || slot.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.pitch = slot.pitch +
            Random.Range(-slot.randomPitchRange, slot.randomPitchRange);
        audioSrc.PlayOneShot(slot.clip, slot.volume);
    }
}