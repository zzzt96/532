using UnityEngine;

public class Switch : ToyBase
{
    [Header("Switch Animation")]
    public Transform leverTransform;
    private Quaternion leverStartRot;
    public float leverFlipAngle = 40f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("开关拨动的清脆'啪嗒'机械声")]
    public SoundSlot switchClickSound;

    [Tooltip("灯亮后持续的'滋滋'电流声 (建议勾选 Loop)")]
    public SoundSlot electricHumSound;
    // ===============================================

    bool activated = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;

        if (leverTransform != null)
            leverStartRot = leverTransform.localRotation;
    }

    public override void ToyUpdate()
    {
        if (activated) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Activate();
        }
    }

    void Activate()
    {
        activated = true;
        Debug.Log("[SwitchL3] Switch activated!");

        // 1. 开关啪嗒声 (一次性)
        PlaySound(switchClickSound);

        if (leverTransform != null)
        {
            Quaternion flipped = Quaternion.Euler(leverFlipAngle, 0f, 0f) * leverStartRot;
            leverTransform.localRotation = flipped;
        }

        // 2. 启动持续电流 loop (灯亮起后一直响, 不再停止)
        // 注意: PlaySound 会自动用 Loop 模式播 (因为 SoundSlot.loop = true)
        PlaySound(electricHumSound);

        Level3Manager.Instance?.OnLightsOn();

        // zoom out 修复: 拨完开关玩家任务结束, 退出附身
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[SwitchL3] Auto-exited possession.");
        }
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[SwitchL3] Possessed - Press Space to turn on lights");
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 注意: 不停 loop! 电流声需要持续, 玩家退出附身后还要响
    }
}