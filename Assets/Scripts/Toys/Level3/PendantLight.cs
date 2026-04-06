using System.Collections;
using UnityEngine;

/// <summary>
/// 吊灯摇晃
/// 挂载位置：LampPivot（天花板挂点空物体）
/// </summary>
public class PendantLight : ToyBase
{
    [Header("Swing Settings")]
    public float triggerAngle = 60f;
    public float amplitudeGrowRate = 15f;
    public float amplitudeDecayRate = 5f;
    public float swingSpeed = 2f;
    public Vector3 swingAxis = Vector3.forward;

    [Header("Return Settings")]
    public float returnDuration = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] float currentAmplitude = 0f;

    [Header("Camera Override")]
    public float pendantPossessFOV = 55f;

    Quaternion initialRotation;
    float swingTime = 0f;
    bool triggered = false;
    
    [Header("Bulb Sync")]
    [Tooltip("灯泡Transform，每帧同步到灯罩位置")]
    public Transform bulb;
    public Transform bulbAnchor; // 灯罩上的一个子空物体，标记灯泡应该在的位置

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        initialRotation = transform.localRotation;
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        bool pressingA = Input.GetKey(KeyCode.A);
        bool pressingD = Input.GetKey(KeyCode.D);

        if (pressingA || pressingD)
        {
            currentAmplitude += amplitudeGrowRate * Time.deltaTime;
            currentAmplitude = Mathf.Clamp(currentAmplitude, 0f, triggerAngle);
        }
        else
        {
            currentAmplitude -= amplitudeDecayRate * Time.deltaTime;
            currentAmplitude = Mathf.Max(0f, currentAmplitude);
        }

        swingTime += swingSpeed * Time.deltaTime;
        // 负号：先向左摆
        float swingAngle = -Mathf.Sin(swingTime) * currentAmplitude;
        transform.localRotation = initialRotation * Quaternion.AngleAxis(swingAngle, swingAxis);

        if (currentAmplitude >= triggerAngle)
        {
            triggered = true;
            Debug.Log("[PendantLight] Trigger reached! Notifying Level3Manager.");
            Level3Manager.Instance?.OnLampSwung();
            StartCoroutine(ReturnToCenter());
        }
        
        // 强制灯泡跟随灯转动！
        if (bulb != null && bulbAnchor != null)
            bulb.position = bulbAnchor.position;
    }

    IEnumerator ReturnToCenter()
    {
        float elapsed = 0f;
        Quaternion currentRot = transform.localRotation;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localRotation = Quaternion.Slerp(currentRot, initialRotation, eased);
            yield return null;
        }
        transform.localRotation = initialRotation;
        Debug.Log("[PendantLight] Returned to center.");
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