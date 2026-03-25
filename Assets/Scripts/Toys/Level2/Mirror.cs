using UnityEngine;

/// <summary>
/// 镜子 - 玩家附身后WASD旋转，SpotLight跟着转
/// 当光方向落入目标角度区间时触发对应谜题阶段
/// </summary>
public class Mirror : ToyBase
{
    // ─── Inspector 设置 ────────────────────────────────────────
    [Header("Rotation")]
    [Tooltip("WASD旋转速度（度/秒）")]
    public float rotateSpeed = 60f;
    [Tooltip("Z轴旋转最小值（限制范围防止乱转）")]
    public float minAngle = -80f;
    [Tooltip("Z轴旋转最大值")]
    public float maxAngle = 80f;

    [Header("SpotLight")]
    [Tooltip("镜子的子物体 SpotLight，和镜子一起转")]
    public Light reflectionLight;

    [Header("Target Zones - Drawer")]
    [Tooltip("光打到抽屉区域的角度范围（Z轴，最小值）")]
    public float drawerAngleMin = 20f;
    [Tooltip("光打到抽屉区域的角度范围（Z轴，最大值）")]
    public float drawerAngleMax = 40f;
    [Tooltip("需要持续照射多少秒才触发")]
    public float holdTimeRequired = 1.0f;

    [Header("Auto Redirect - Wardrobe")]
    [Tooltip("抽屉打开后，镜子自动转到的角度（照向衣柜顶）")]
    public float wardrobeRedirectAngle = -30f;
    [Tooltip("自动转向的速度")]
    public float autoRotateSpeed = 45f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private float currentAngle = 0f;           // 当前Z轴旋转角度
    private float drawerHoldTimer = 0f;        // 持续照射抽屉的计时器
    private bool drawerTriggered = false;      // 是否已触发抽屉阶段
    private bool autoRedirecting = false;      // 是否正在自动转向衣柜
    private float autoRedirectTarget = 0f;
    
    // ToyBase 覆写
    public override void ToyUpdate()
    {
        if (autoRedirecting)
        {
            DoAutoRedirect();
            return;
        }

        HandleRotationInput();
        CheckDrawerZone();
    }

    public override void Possess()
    {
        base.Possess();
        if (reflectionLight != null) reflectionLight.enabled = true;
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 脱离附身后保留光（还在照着），玩家只是失去控制
    }
    
    // 私有方法
    void HandleRotationInput()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // 旋转镜子本身（Z轴）
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    void CheckDrawerZone()
    {
        if (drawerTriggered) return;

        bool inDrawerZone = currentAngle >= drawerAngleMin && currentAngle <= drawerAngleMax;

        if (inDrawerZone)
        {
            drawerHoldTimer += Time.deltaTime;
            if (drawerHoldTimer >= holdTimeRequired)
            {
                drawerTriggered = true;
                Debug.Log("[Mirror] Light aimed at drawer zone! Triggering.");
                Level2Manager.Instance?.OnMirrorAimedAtDrawer();
            }
        }
        else
        {
            // 离开区域，重置计时（需要连续照射）
            drawerHoldTimer = 0f;
        }
    }

    void DoAutoRedirect()
    {
        currentAngle = Mathf.MoveTowards(currentAngle, autoRedirectTarget, autoRotateSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        if (Mathf.Abs(currentAngle - autoRedirectTarget) < 0.5f)
        {
            currentAngle = autoRedirectTarget;
            autoRedirecting = false;
            Debug.Log("[Mirror] Auto-redirected to wardrobe angle.");
        }
    }

    // ════════════════════════════════════════════════════════════
    // 外部接口
    // ════════════════════════════════════════════════════════════

    /// <summary>抽屉打开后由Level2Manager调用，光自动转向衣柜顶</summary>
    public void AutoRedirectToWardrobe()
    {
        autoRedirecting = true;
        autoRedirectTarget = wardrobeRedirectAngle;
        canBePossessed = false; // 这时候玩家不能再控制镜子了
        Debug.Log("[Mirror] Starting auto-redirect to wardrobe.");
    }

    // ════════════════════════════════════════════════════════════
    // 编辑器辅助
    // ════════════════════════════════════════════════════════════
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        // 在Scene视图里画出两个目标角度区间（绿色=抽屉，蓝色=衣柜）
        Gizmos.color = Color.green;
        DrawAngleGizmo(drawerAngleMin);
        DrawAngleGizmo(drawerAngleMax);
        Gizmos.color = Color.cyan;
        DrawAngleGizmo(wardrobeRedirectAngle);
    }

    void DrawAngleGizmo(float angle)
    {
        float rad = (angle - 90f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Gizmos.DrawRay(transform.position, dir * 3f);
    }
}