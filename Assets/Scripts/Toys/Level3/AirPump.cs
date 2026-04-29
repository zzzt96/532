using UnityEngine;

public class AirPump : ToyBase
{
    [Header("Pump Settings")]
    public float fillDuration = 3.5f;

    [Header("Pump Animation")]
    public Transform piston;
    public float pistonPressDistance = 0.3f;

    [Header("Balloon Reference")]
    public BalloonL3 balloon;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("打气筒'呲呲'节奏充气声 (建议勾选 Loop, 按住 Space 时持续)")]
    public SoundSlot pumpSound;
    // ===============================================

    float fillTimer = 0f;
    bool filled = false;
    Vector3 pistonStartPos;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        if (piston != null) pistonStartPos = piston.localPosition;
    }

    public override void UnPossess()
    {
        base.UnPossess();
        if (piston != null) piston.localPosition = pistonStartPos;
        StopSound();
    }

    public override void ToyUpdate()
    {
        if (filled) return;

        if (Input.GetKey(KeyCode.Space))
        {
            fillTimer += Time.deltaTime;

            if (piston != null)
            {
                float press = Mathf.Sin(Time.time * 8f) * pistonPressDistance * 0.5f + pistonPressDistance * 0.5f;
                piston.localPosition = pistonStartPos + Vector3.left * press;
            }

            float progress = Mathf.Clamp01(fillTimer / fillDuration);
            balloon?.UpdateInflationProgress(progress);

            // 充气期间持续播 loop
            PlaySound(pumpSound);

            if (fillTimer >= fillDuration)
            {
                filled = true;
                if (piston != null) piston.localPosition = pistonStartPos;

                // 停止充气声
                StopSound();

                Debug.Log("[AirPump] Balloon filled!");
                Level3Manager.Instance?.OnBalloonFilled();

                // zoom out 修复: 充满气球后玩家任务结束
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && player.isPossessing && player.currentToy == this)
                {
                    player.ExitPossess();
                    Debug.Log("[AirPump] Auto-exited possession.");
                }
            }
        }
        else
        {
            // 松开 Space: 活塞复位 + 停止音效
            if (piston != null)
                piston.localPosition = Vector3.Lerp(piston.localPosition, pistonStartPos, Time.deltaTime * 10f);
            StopSound();
        }
    }

    public override void Possess()
    {
        base.Possess();
        fillTimer = 0f;
        Debug.Log("[AirPump] Possessed - Hold Space to pump!");
    }
}