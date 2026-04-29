using System.Collections;
using UnityEngine;

public class Blank : MonoBehaviour
{
    [Header("Fall Settings")]
    public Vector3 targetWorldRotation = new Vector3(3f, -18.6f, -0.4f);
    public Vector3 targetWorldPosition;
    public float fallDuration = 0.6f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("木板倒地的木质'哐/砰'声 (倒下完成瞬间播放)")]
    public SoundSlot plankFallSound;
    // ===============================================

    Quaternion startRot;
    Vector3 startPos;
    AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        startRot = transform.rotation;
        startPos = transform.position;
    }

    public void TriggerFall()
    {
        StartCoroutine(FallRoutine());
    }

    IEnumerator FallRoutine()
    {
        Quaternion endRot = Quaternion.Euler(targetWorldRotation);
        Vector3 endPos = targetWorldPosition;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.rotation = endRot;
        transform.position = endPos;

        // 木板倒地瞬间
        PlayOneShotSlot(plankFallSound);

        Debug.Log("[Blank] Plank fell, puddle covered.");
        Level3Manager.Instance?.OnPlankFell();
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