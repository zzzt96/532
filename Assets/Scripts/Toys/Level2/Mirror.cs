using UnityEngine;

public class Mirror : ToyBase
{
    [Header("Rotation")]
    public float rotateSpeed = 60f;
    public float minAngle = -80f;
    public float maxAngle = 80f;

    [Header("SpotLight")]
    public Light reflectionLight;

    [Header("Beam")]
    public GameObject mirrorBeam;

    [Header("Zone 1 - Drawer")]
    public float drawerAngleMin = 15f;
    public float drawerAngleMax = 35f;
    public float holdTimeRequired = 1.0f;

    [Header("Zone 2 - Wardrobe Top")]
    public float wardrobeAngleMin = 50f;
    public float wardrobeAngleMax = 75f;

    [Header("Target Indicators")]
    [Tooltip("抽屉位置的瞄准点 (一个发光的小物体, 引导玩家'要把光照到这里')")]
    public GameObject drawerTargetIndicator;

    [Tooltip("衣柜顶位置的瞄准点 (第一阶段完成后才显示)")]
    public GameObject wardrobeTargetIndicator;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("木质摩擦+齿轮咯吱声 (按 AD 旋转时持续 loop)")]
    public SoundSlot mirrorRotateSound;
    // ===============================================

    private float currentAngle = 0f;
    private float holdTimer = 0f;
    private Quaternion initialRotation;

    private bool drawerTriggered = false;
    private bool wardrobeTriggered = false;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.rotation;
        canBePossessed = false;

        // 初始状态: 抽屉标记亮, 衣柜标记暗
        if (drawerTargetIndicator != null) drawerTargetIndicator.SetActive(true);
        if (wardrobeTargetIndicator != null) wardrobeTargetIndicator.SetActive(false);
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
        // 玩家退出附身时停止旋转音 (光束保留)
        StopSound();
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
        // 只接受 WASD, 不接受方向键
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, currentAngle);

        if (Mathf.Abs(input) > 0.01f)
            PlaySound(mirrorRotateSound);
        else
            StopSound();
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

        // 视觉反馈: 抽屉标记暗掉, 衣柜标记亮起 (引导玩家看到"还有一个目标")
        if (drawerTargetIndicator != null) drawerTargetIndicator.SetActive(false);
        if (wardrobeTargetIndicator != null) wardrobeTargetIndicator.SetActive(true);
    }

    void OnWardrobeZoneHeld()
    {
        wardrobeTriggered = true;
        canBePossessed = false;
        Debug.Log("[Mirror] Zone 2: Wardrobe triggered!");
        Level2Manager.Instance?.OnMirrorAimedAtWardrobe();

        // 视觉反馈: 衣柜标记也暗掉, 镜子任务全部完成
        if (wardrobeTargetIndicator != null) wardrobeTargetIndicator.SetActive(false);

        // zoom out: 第二阶段完成后玩家任务结束, 退出附身
        StopSound();
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[Mirror] Auto-exited possession after wardrobe phase.");
        }
    }

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