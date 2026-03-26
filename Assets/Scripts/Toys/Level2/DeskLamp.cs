using UnityEngine;

/// <summary>
/// 台灯 - 玩家附身后 AD 键旋转
/// 旋转基于初始朝向叠加，不会附身瞬间跳位
/// 附身时光束激活，照到纸盒子区域持续一段时间后猫跳上去
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

    [Header("Lamp Beam")]
    public GameObject lampBeam;

    [Header("Album Box Zone")]
    public float boxAngleMin = -65f;
    public float boxAngleMax = -25f;
    public float holdTimeRequired = 0.8f;

    private float currentAngle = 0f;
    private float holdTimer = 0f;
    private bool triggered = false;
    private Quaternion initialRotation;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.localRotation; 
        canBePossessed = false;
    }

    public override void Possess()
    {
        base.Possess();
        if (lampSpotLight != null) lampSpotLight.enabled = true;
        if (lampBeam != null) lampBeam.SetActive(true);
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log($"[Lamp] current angle: {currentAngle}");
        
        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // 基于初始朝向叠加旋转，不覆盖原始角度
        transform.localRotation = initialRotation * Quaternion.Euler(0f, currentAngle, 0f);
    
        bool inZone = currentAngle >= boxAngleMin && currentAngle <= boxAngleMax;
        holdTimer = inZone ? holdTimer + Time.deltaTime : 0f;

        if (holdTimer >= holdTimeRequired)
        {
            triggered = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            Debug.Log("[Lamp] Light on box! Cat jumping.");
            Level2Manager.Instance?.cat?.GoToAlbumBox();
        }
    }
}