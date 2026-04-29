using UnityEngine;

public class PendantLight : ToyBase
{
    [Header("Pivot")]
    public Transform pivotTransform;

    [Header("Swing Settings")]
    public float triggerAngle = 60f;
    public float amplitudeGrowRate = 15f;
    public float amplitudeDecayRate = 5f;
    public float swingSpeed = 2f;
    public Vector3 swingAxis = Vector3.forward;

    [Header("Post-Trigger Decay")]
    [Tooltip("触发后吊灯自动衰减到停止的速度 (越大停得越快)")]
    public float postTriggerDecayRate = 8f;

    [Header("Visual Feedback")]
    [SerializeField] float currentAmplitude = 0f;

    [Header("Camera Override")]
    public float pendantPossessFOV = 55f;

    Quaternion initialRotation;
    float swingTime = 0f;
    bool triggered = false;

    Transform RotTarget => pivotTransform != null ? pivotTransform : transform;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        initialRotation = RotTarget.localRotation;
    }

    void Update()
    {
        // 触发后吊灯继续摆动 + 幅度自然衰减 (即使玩家不在附身状态也要执行)
        if (triggered && currentAmplitude > 0.01f)
        {
            // 幅度衰减
            currentAmplitude -= postTriggerDecayRate * Time.deltaTime;
            currentAmplitude = Mathf.Max(0f, currentAmplitude);

            // 继续做 sin 摆动
            swingTime += swingSpeed * Time.deltaTime;
            float swingAngle = -Mathf.Sin(swingTime) * currentAmplitude;
            RotTarget.localRotation = initialRotation * Quaternion.AngleAxis(swingAngle, swingAxis);
        }
        else if (triggered && currentAmplitude <= 0.01f)
        {
            // 完全停止: 归位到垂直
            RotTarget.localRotation = initialRotation;
        }
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        bool pressingA = Input.GetKey(KeyCode.A);
        bool pressingD = Input.GetKey(KeyCode.D);

        if (pressingA)
        {
            currentAmplitude += amplitudeGrowRate * Time.deltaTime;
            currentAmplitude = Mathf.Clamp(currentAmplitude, 0f, triggerAngle);
        }
        else if (pressingD)
        {
            currentAmplitude -= amplitudeDecayRate * Time.deltaTime;
            currentAmplitude = Mathf.Max(0f, currentAmplitude);
        }

        swingTime += swingSpeed * Time.deltaTime;
        float swingAngle = -Mathf.Sin(swingTime) * currentAmplitude;
        RotTarget.localRotation = initialRotation * Quaternion.AngleAxis(swingAngle, swingAxis);

        if (currentAmplitude >= triggerAngle)
        {
            triggered = true;
            // 注意: 让 Update 接管, 自然衰减
            Debug.Log("[PendantLight] Trigger reached! Lamp continues swinging and decays naturally.");
            Level3Manager.Instance?.OnLampSwung();

            // zoom out 修复: 灯达到触发幅度后玩家任务结束
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.isPossessing && player.currentToy == this)
            {
                player.ExitPossess();
                Debug.Log("[PendantLight] Auto-exited possession.");
            }
        }
    }

    public override void Possess()
    {
        base.Possess();
        swingTime = 0f;
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.possessFOV = pendantPossessFOV;
        Debug.Log("[PendantLight] Possessed - Press A/D to swing!");
    }

    public override void UnPossess()
    {
        base.UnPossess();
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.possessFOV = 35f;
    }
}