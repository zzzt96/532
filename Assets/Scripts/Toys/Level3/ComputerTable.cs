using System.Collections;
using UnityEngine;

public class ComputerTable : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 1.0f;
    public float shakeAmplitude = 0.08f;
    public float shakeFrequency = 25f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("桌子共振的低频'嗡/咚'震动声")]
    public SoundSlot tableResonanceSound;
    // ===============================================

    Vector3 startLocalPos;
    AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        startLocalPos = transform.localPosition;
    }

    public void TriggerShake()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        // 桌子共振音效 (一次性, 跟震动同步)
        PlayOneShotSlot(tableResonanceSound);

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float amp = shakeAmplitude * Mathf.Sin(progress * Mathf.PI);
            float offsetX = Mathf.Sin(Time.time * shakeFrequency) * amp;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 1.5f) * amp * 0.4f;
            transform.localPosition = startLocalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        transform.localPosition = startLocalPos;
        Debug.Log("[ComputerTable] Shake done.");
        Level3Manager.Instance?.OnTableShakeDone();
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