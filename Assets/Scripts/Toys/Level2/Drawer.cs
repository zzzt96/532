using UnityEngine;
using System.Collections;

public class Drawer : MonoBehaviour
{
    [Header("Open Animation")]
    public Vector3 openOffset = new Vector3(0.6f, 0f, 0f);
    public float openDuration = 0.4f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("抽屉打开声 (木制滑轨沙沙+轻微卡顿)")]
    public SoundSlot openSound;
    // ===============================================

    private bool isOpen = false;
    private Vector3 closedLocalPos;
    private AudioSource audioSrc;

    void Awake()
    {
        closedLocalPos = transform.localPosition;
    }

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        // 开始打开瞬间播放音效
        PlayOneShotSlot(openSound);

        Vector3 openLocalPos = closedLocalPos + openOffset;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            transform.localPosition = Vector3.Lerp(closedLocalPos, openLocalPos, t);
            yield return null;
        }
        transform.localPosition = openLocalPos;

        Debug.Log("[Drawer] Opened! Notifying Level2Manager.");
        Level2Manager.Instance?.OnDrawerOpened();
    }

    public void OpenByWeight()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenRoutine());
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