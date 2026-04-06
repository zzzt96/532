using System.Collections;
using UnityEngine;

/// <summary>
/// 易拉罐组 - 保龄球式散开
/// 自动检测所有子物体，支持单独设置特定罐子的Drop Y
/// </summary>
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
    [Tooltip("默认Y轴下落距离，0=不下落")]
    public float dropY = 0f;

    [Header("Special Drop Y（叠放罐子单独设置）")]
    [Tooltip("需要额外下落的罐子，单独设置Drop Y")]
    public SpecialDrop[] specialDrops;

    [System.Serializable]
    public class SpecialDrop
    {
        public Transform canTransform;
        [Tooltip("这个罐子单独的Drop Y")]
        public float dropY = 0.5f;
    }

    Transform[] canObjects;
    bool hasScattered = false;

    void Start()
    {
        canObjects = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            canObjects[i] = transform.GetChild(i);

        Debug.Log($"[Cans] Auto-detected {canObjects.Length} cans.");
    }

    public void TriggerScatter()
    {
        if (hasScattered) return;
        hasScattered = true;
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

            // 查找是否有特殊Drop Y设置 -- 目前场景里有两个！
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
}