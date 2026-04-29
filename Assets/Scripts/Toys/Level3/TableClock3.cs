using UnityEngine;

public class TableClock3 : ToyBase
{
    [Header("Shake Settings")]
    public float shakeRequiredTime = 2f;
    public float shakeAmplitude = 0.15f;
    public float shakeFrequency = 30f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("闹钟震动的高频'嗡嗡'声 (建议勾选 Loop, 按住 Space 时持续)")]
    public SoundSlot clockBuzzSound;
    // ===============================================

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

            float progress = Mathf.Clamp01(shakeTimer / shakeRequiredTime);
            float currentAmplitude = shakeAmplitude * (0.5f + progress * 0.5f);

            float offsetX = Mathf.Sin(Time.time * shakeFrequency) * currentAmplitude;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 1.3f) * currentAmplitude * 0.5f;
            transform.localPosition = startLocalPos + new Vector3(offsetX, offsetY, 0f);

            // 震动期间持续播 loop
            PlaySound(clockBuzzSound);

            if (shakeTimer >= shakeRequiredTime)
            {
                triggered = true;
                transform.localPosition = startLocalPos;

                // 停止震动 loop
                StopSound();

                Debug.Log("[TableClock] Shake complete! Triggering table.");
                Level3Manager.Instance?.OnClockShaken();

                // zoom out 修复: 触发桌子震动演出, 玩家退出附身
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && player.isPossessing && player.currentToy == this)
                {
                    player.ExitPossess();
                    Debug.Log("[TableClock] Auto-exited possession.");
                }
            }
        }
        else
        {
            // 松开 Space: 归位 + 停止震动音
            shakeTimer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, startLocalPos, Time.deltaTime * 10f);
            StopSound();
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
        StopSound();
    }
}