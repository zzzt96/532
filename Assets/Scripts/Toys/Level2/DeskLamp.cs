using UnityEngine;

/// <summary>
/// 台灯 - 玩家附身后 AD 键旋转灯的方向
/// 光照角度落入木箱区域并持续一段时间后，吸引猫跳上木箱
/// 逻辑和 Mirror.cs 相同，但是台灯（点光源而非反射）
/// </summary>
public class DeskLamp : ToyBase
{
    [Header("Rotation")]
    public float rotateSpeed = 60f;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Light")]
    [Tooltip("台灯的 SpotLight 子物体，附身时打开")]
    public Light lampSpotLight;

    [Header("Album Box Zone")]
    [Tooltip("灯照到木箱时的 Z 旋转角度下限")]
    public float boxAngleMin = -65f;
    [Tooltip("灯照到木箱时的 Z 旋转角度上限")]
    public float boxAngleMax = -25f;
    [Tooltip("需要持续照射多少秒才触发")]
    public float holdTimeRequired = 0.8f;

    private float currentAngle = 0f;
    private float holdTimer = 0f;
    private bool triggered = false;

    // 默认不可附身，由 Level2Manager 在合适时机解锁
    void Start()
    {
        canBePossessed = false;
    }

    public override void Possess()
    {
        base.Possess();
        if (lampSpotLight != null) lampSpotLight.enabled = true;
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        bool inZone = currentAngle >= boxAngleMin && currentAngle <= boxAngleMax;
        holdTimer = inZone ? holdTimer + Time.deltaTime : 0f;

        if (holdTimer >= holdTimeRequired)
        {
            triggered = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            Debug.Log("[Lamp] Light on box! Attracting cat.");
            // 直接驱动猫，不经过 Level2Manager（猫是 Level2Manager 的引用）
            Level2Manager.Instance?.cat?.GoToAlbumBox();
        }
    }
}