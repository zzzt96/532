using UnityEngine;

/// <summary>
/// 桌上小镜子 - 玩家附身后AD旋转，SpotLight跟着转
/// 阶段1：光打到抽屉 → 猫从桌面跳到抽屉
/// 阶段2：玩家继续旋转（不退出附身）→ 光打到柜顶 → 猫跳上柜顶
/// 整个过程玩家始终附身镜子
/// </summary>
public class Mirror : ToyBase
{
    [Header("Rotation")]
    public float rotateSpeed = 60f;
    public float minAngle = -80f;
    public float maxAngle = 80f;

    [Header("SpotLight")]
    [Tooltip("镜子的子物体 SpotLight，随镜子一起旋转")]
    public Light reflectionLight;

    [Header("Beam")]
    public GameObject mirrorBeam;
    
    [Header("Zone 1 - Drawer")]
    [Tooltip("光打到抽屉时的角度范围 最小值（场景里测出来填）")]
    public float drawerAngleMin = 15f;
    public float drawerAngleMax = 35f;
    [Tooltip("需要持续照射多少秒才触发")]
    public float holdTimeRequired = 1.0f;

    [Header("Zone 2 - Wardrobe Top")]
    [Tooltip("光打到柜顶时的角度范围 最小值（抽屉触发后才检测这个）")]
    public float wardrobeAngleMin = 50f;
    public float wardrobeAngleMax = 75f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;

    // ─── 私有状态 ──────────────────────────────────────────────
    private float currentAngle = 0f;
    private float holdTimer = 0f;
    private Quaternion initialRotation;

    // 两个阶段的触发状态
    private bool drawerTriggered = false;
    private bool wardrobeTriggered = false;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.rotation; // 记录场景里摆好的初始朝向
        canBePossessed = false;
    }
    public override void Possess()
    {
        base.Possess();
        if (reflectionLight != null) reflectionLight.enabled = true;
        if (mirrorBeam != null) mirrorBeam.SetActive(true);
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 光束保留，玩家退出附身后光还在照着
    }
    public override void ToyUpdate()
    {
        HandleRotationInput();

        if (!drawerTriggered)
            CheckZone(drawerAngleMin, drawerAngleMax, ref holdTimer, OnDrawerZoneHeld);
        else if (!wardrobeTriggered)
            CheckZone(wardrobeAngleMin, wardrobeAngleMax, ref holdTimer, OnWardrobeZoneHeld);
    }

  void HandleRotationInput()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        
        transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, currentAngle);
    }
    void CheckZone(float min, float max, ref float timer, System.Action onTriggered)
    {
        bool inZone = currentAngle >= min && currentAngle <= max;
        timer = inZone ? timer + Time.deltaTime : 0f;
        if (inZone && timer >= holdTimeRequired) 
        {
            timer = 0f;
            onTriggered?.Invoke();
        }
    }

    void OnDrawerZoneHeld()
    {
        drawerTriggered = true;
        Debug.Log("[Mirror] Zone 1: Drawer triggered!");
        Level2Manager.Instance?.OnMirrorAimedAtDrawer();
        // 玩家不退出附身，继续旋转进入阶段2
    }
    
    void OnWardrobeZoneHeld()
    {
        wardrobeTriggered = true;
        canBePossessed = false;
        Debug.Log("[Mirror] Zone 2: Wardrobe triggered!");
        Level2Manager.Instance?.OnMirrorAimedAtWardrobe(); // 改这里
    }
    
    /// <summary>外部调用接口保留（Level2Manager里有引用）</summary>
    public void AutoRedirectToWardrobe() { /* 新流程不需要自动转，玩家手动转 */ }

    // ════════════════════════════════════════════════════════════
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        Gizmos.color = Color.green;
        DrawAngleGizmo(drawerAngleMin);
        DrawAngleGizmo(drawerAngleMax);
        Gizmos.color = Color.cyan;
        DrawAngleGizmo(wardrobeAngleMin);
        DrawAngleGizmo(wardrobeAngleMax);
    }

    void DrawAngleGizmo(float angle)
    {
        float rad = (angle - 90f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Gizmos.DrawRay(transform.position, dir * 3f);
    }
}