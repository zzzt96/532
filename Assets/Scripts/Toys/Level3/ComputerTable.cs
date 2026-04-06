using System.Collections;
using UnityEngine;

/// <summary>
/// 桌子震动脚本
/// 由Level3Manager调用TriggerShake()
/// 震动一段时间后自动停止，并通知Manager木板可以倒下
/// </summary>
public class ComputerTable : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 1.0f;
    public float shakeAmplitude = 0.08f;
    public float shakeFrequency = 25f;

    Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    public void TriggerShake()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            // 震动幅度先增后减
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
}