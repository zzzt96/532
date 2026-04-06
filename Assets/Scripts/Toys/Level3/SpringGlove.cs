using UnityEngine;

public class SpringGlove : ToyBase
{
    [Header("子物体引用（拖入）")]
    [Tooltip("短弹簧GameObject（初始显示）")]
    public GameObject springShort;
    [Tooltip("长弹簧GameObject（出拳时显示，初始Disable）")]
    public GameObject springLong;
    [Tooltip("手套子物体 toy（跟随弹簧末端）")]
    public Transform glove;

    [Header("Punch Settings")]
    [Tooltip("手套移动方向（world space），向左填(-1,0,0)")]
    public Vector3 glovePunchDirection = new Vector3(-1f, 0f, 0f);
    [Tooltip("手套移动距离")]
    public float gloveTravelDistance = 3f;
    [Tooltip("出拳速度")]
    public float punchSpeed = 8f;
    [Tooltip("缩回速度")]
    public float retractSpeed = 5f;

    [Header("Cans Hit Detection")]
    [Tooltip("拖入Cans GameObject")]
    public Cans cans;
    [Tooltip("击中判定距离")]
    public float hitDetectRange = 1.2f;

    // 内部状态
    enum GloveState { Idle, Punching, Retracting }
    GloveState gloveState = GloveState.Idle;

    // 记录初始值
    Vector3 gloveStartLocalPos;
    Vector3 gloveTargetLocalPos;

    float punchProgress = 0f;   // 0→1 出拳进度
    bool hasHit = false;
    bool punchUsed = false;

    protected override void Start()
    {
        base.Start();

        if (glove == null) Debug.LogError("[SpringGlove] glove 未分配！Inspector里把toy拖进去。");

        // 初始状态：短弹簧显示，长弹簧隐藏
        if (springShort != null) springShort.SetActive(true);
        if (springLong != null)  springLong.SetActive(false);

        if (glove != null)
        {
            gloveStartLocalPos = glove.position;
            gloveTargetLocalPos = glove.position + glovePunchDirection.normalized * gloveTravelDistance;
            Debug.Log($"[SpringGlove] Glove: {gloveStartLocalPos} → {gloveTargetLocalPos}");
        }
    }

    public override void ToyUpdate()
    {
        if (punchUsed) return;

        switch (gloveState)
        {
            case GloveState.Idle:
                if (Input.GetKeyDown(KeyCode.Space))
                    StartPunch();
                break;

            case GloveState.Punching:
                UpdatePunch();
                break;

            case GloveState.Retracting:
                UpdateRetract();
                break;
        }
    }

    void StartPunch()
    {
        gloveState = GloveState.Punching;
        punchProgress = 0f;
        hasHit = false;
        Debug.Log("[SpringGlove] Punch!");
    }

    void UpdatePunch()
    {
        punchProgress += punchSpeed * Time.deltaTime;
        float t = Mathf.Clamp01(punchProgress);

        // 出拳开始时：切换到长弹簧
        if (springShort != null) springShort.SetActive(false);
        if (springLong != null)  springLong.SetActive(true);

        // 手套位移（world space）
        if (glove != null)
            glove.position = Vector3.Lerp(gloveStartLocalPos, gloveTargetLocalPos, t);
        
        if (!hasHit && cans != null && glove != null)
        {
            float dist = Vector3.Distance(glove.position, cans.transform.position);
            if (dist <= hitDetectRange)
            {
                hasHit = true;
                OnHitCans();
            }
        }

        if (t >= 1f)
        {
            punchProgress = 0f;
            gloveState = GloveState.Retracting;
        }
    }

    void UpdateRetract()
    {
        punchProgress += retractSpeed * Time.deltaTime;
        float t = Mathf.Clamp01(punchProgress);

        if (glove != null)
            glove.position = Vector3.Lerp(gloveTargetLocalPos, gloveStartLocalPos, t);

        if (t >= 1f)
        {
            if (glove != null) glove.position = gloveStartLocalPos;

            // 缩回后：切回短弹簧
            if (springShort != null) springShort.SetActive(true);
            if (springLong != null)  springLong.SetActive(false);

            gloveState = GloveState.Idle;

            if (hasHit)
            {
                punchUsed = true;
                Debug.Log("[SpringGlove] Punch complete, glove locked.");
            }
        }
    }

    void OnHitCans()
    {
        Debug.Log("[SpringGlove] Hit Cans!");
        cans?.TriggerScatter();
        Level3Manager.Instance?.OnCansCleared();
    }

    public override void Possess()
    {
        base.Possess();
        gloveState = GloveState.Idle;
        punchProgress = 0f;
    }
}