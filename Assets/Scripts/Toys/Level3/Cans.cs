using System.Collections;
using UnityEngine;

public class Cans : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fallDuration = 0.4f;
    public float maxDelay = 0.06f;
    public float fallAngle = 90f;
    public float slideDistance = 0.35f;

    [Header("Scatter Direction")]
    public Vector3 impactDirection = Vector3.right;
    public float scatterAngle = 160f;

    [Header("Default Drop Y")]
    public float dropY = 0f;

    [Header("Special Drop Y")]
    public SpecialDrop[] specialDrops;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("易拉罐被撞散落的连续金属当啷哐啷声")]
    public SoundSlot scatterSound;
    // ===============================================

    [System.Serializable]
    public class SpecialDrop
    {
        public Transform canTransform;
        public float dropY = 0.5f;
    }

    Transform[] canObjects;
    bool hasScattered = false;
    AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();

        canObjects = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            canObjects[i] = transform.GetChild(i);

        Debug.Log($"[Cans] Auto-detected {canObjects.Length} cans.");
    }

    public void TriggerScatter()
    {
        if (hasScattered) return;
        hasScattered = true;

        // 散落音效 (一次性, 涵盖整个散落过程)
        PlayOneShotSlot(scatterSound);

        StartCoroutine(ScatterRoutine());
    }

    IEnumerator ScatterRoutine()
    {
        int count = canObjects.Length;
        if (count == 0) yield break;

        for (int i = 0; i < count; i++)
        {
            if (canObjects[i] == null) continue;

            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-scatterAngle / 2f, scatterAngle / 2f, t);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * impactDirection.normalized;

            float thisDropY = dropY;
            foreach (var special in specialDrops)
            {
                if (special.canTransform == canObjects[i])
                {
                    thisDropY = special.dropY;
                    break;
                }
            }

            StartCoroutine(FallOne(canObjects[i], dir, thisDropY));
            yield return new WaitForSeconds(Random.Range(0f, maxDelay));
        }
    }

    IEnumerator FallOne(Transform can, Vector3 fallDir, float thisDropY)
    {
        Vector3 startPos = can.localPosition;
        Quaternion startRot = can.localRotation;

        Vector3 rotAxis = Vector3.Cross(Vector3.up, fallDir.normalized);
        if (rotAxis.sqrMagnitude < 0.01f) rotAxis = Vector3.forward;

        Quaternion endRot = Quaternion.AngleAxis(fallAngle, rotAxis) * startRot;
        Vector3 endPos = startPos + fallDir.normalized * slideDistance + Vector3.down * thisDropY;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t01 = elapsed / fallDuration;
            float eased = 1f - Mathf.Pow(1f - t01, 3f);

            can.localPosition = Vector3.Lerp(startPos, endPos, eased);
            can.localRotation = Quaternion.Slerp(startRot, endRot, eased);

            elapsed += Time.deltaTime;
            yield return null;
        }

        can.localPosition = endPos;
        can.localRotation = endRot;
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