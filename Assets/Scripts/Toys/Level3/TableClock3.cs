using UnityEngine;

/// <summary>
/// 桌上闹钟
/// 附身后按住Space → 闹钟震动（越来越剧烈）
/// 震动持续一定时间 → 通知Level3Manager触发桌子震动+木板倒下
/// </summary>
public class TableClock3 : ToyBase
{
    [Header("Shake Settings")]
    [Tooltip("需要持续震动多少秒才触发")]
    public float shakeRequiredTime = 2f;
    [Tooltip("震动幅度（位移）")]
    public float shakeAmplitude = 0.15f;
    [Tooltip("震动频率")]
    public float shakeFrequency = 30f;

    Vector3 startLocalPos;
    float shakeTimer = 0f;
    bool triggered = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        startLocalPos = transform.localPosition;
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        if (Input.GetKey(KeyCode.Space))
        {
            shakeTimer += Time.deltaTime;

            // 震动幅度随时间增加
            float progress = Mathf.Clamp01(shakeTimer / shakeRequiredTime);
            float currentAmplitude = shakeAmplitude * (0.5f + progress * 0.5f);

            // 随机震动偏移
            float offsetX = Mathf.Sin(Time.time * shakeFrequency) * currentAmplitude;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 1.3f) * currentAmplitude * 0.5f;
            transform.localPosition = startLocalPos + new Vector3(offsetX, offsetY, 0f);

            if (shakeTimer >= shakeRequiredTime)
            {
                triggered = true;
                transform.localPosition = startLocalPos;
                Debug.Log("[TableClock] Shake complete! Triggering table.");
                Level3Manager.Instance?.OnClockShaken();
            }
        }
        else
        {
            // 松开Space：归位
            shakeTimer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, startLocalPos, Time.deltaTime * 10f);
        }
    }

    public override void Possess()
    {
        base.Possess();
        shakeTimer = 0f;
        Debug.Log("[TableClock] Possessed - Hold Space to shake!");
    }

    public override void UnPossess()
    {
        base.UnPossess();
        transform.localPosition = startLocalPos;
    }
}