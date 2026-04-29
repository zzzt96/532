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
            RotTarget.localRotation = initialRotation * Quaternion.AngleAxis(triggerAngle, swingAxis);
            Debug.Log("[PendantLight] Trigger reached! Lamp stays tilted left.");
            Level3Manager.Instance?.OnLampSwung();

            // zoom out 修复: 灯摆到位后, 玩家任务结束
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