using System.Collections;
using UnityEngine;

public class Blank : MonoBehaviour
{
    [Header("Fall Settings")]
    [Tooltip("木板倒下后的目标世界旋转（在Editor里手动摆好位置后填入）")]
    public Vector3 targetWorldRotation = new Vector3(3f, -18.6f, -0.4f);
    [Tooltip("木板倒下后的目标世界位置（在Editor里手动摆好位置后填入）")]
    public Vector3 targetWorldPosition;
    public float fallDuration = 0.6f;

    Quaternion startRot;
    Vector3 startPos;

    void Start()
    {
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

        Debug.Log("[Blank] Plank fell, puddle covered.");
        Level3Manager.Instance?.OnPlankFell();
    }
}