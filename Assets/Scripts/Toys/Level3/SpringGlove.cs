using UnityEngine;

public class SpringGlove : ToyBase
{
    [Header("子物体引用（拖入）")]
    public GameObject springShort;
    public GameObject springLong;
    public Transform glove;

    [Header("Punch Settings")]
    public Vector3 glovePunchDirection = new Vector3(-1f, 0f, 0f);
    public float gloveTravelDistance = 3f;
    public float punchSpeed = 8f;
    public float retractSpeed = 5f;

    [Header("Cans Hit Detection")]
    public Cans cans;
    public float hitDetectRange = 1.2f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("弹簧弹出的'嘣'声 (按 Space 出拳瞬间)")]
    public SoundSlot punchOutSound;

    [Tooltip("击中易拉罐的轻质撞击'砰/哐'声")]
    public SoundSlot hitCansSound;
    // ===============================================

    enum GloveState { Idle, Punching, Retracting }
    GloveState gloveState = GloveState.Idle;

    Vector3 gloveStartLocalPos;
    Vector3 gloveTargetLocalPos;

    float punchProgress = 0f;
    bool hasHit = false;
    bool punchUsed = false;

    protected override void Start()
    {
        base.Start();

        if (glove == null) Debug.LogError("[SpringGlove] glove 未分配！");

        if (springShort != null) springShort.SetActive(true);
        if (springLong != null)  springLong.SetActive(false);

        if (glove != null)
        {
            gloveStartLocalPos = glove.position;
            gloveTargetLocalPos = glove.position + glovePunchDirection.normalized * gloveTravelDistance;
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

        // 弹簧弹出声
        PlaySound(punchOutSound);

        Debug.Log("[SpringGlove] Punch!");
    }

    void UpdatePunch()
    {
        punchProgress += punchSpeed * Time.deltaTime;
        float t = Mathf.Clamp01(punchProgress);

        if (springShort != null) springShort.SetActive(false);
        if (springLong != null)  springLong.SetActive(true);

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

            if (springShort != null) springShort.SetActive(true);
            if (springLong != null)  springLong.SetActive(false);

            gloveState = GloveState.Idle;

            if (hasHit)
            {
                punchUsed = true;
                Debug.Log("[SpringGlove] Punch complete, glove locked.");

                // zoom out 修复: 拳击任务完成, 玩家退出附身
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && player.isPossessing && player.currentToy == this)
                {
                    player.ExitPossess();
                    Debug.Log("[SpringGlove] Auto-exited possession.");
                }
            }
        }
    }

    void OnHitCans()
    {
        Debug.Log("[SpringGlove] Hit Cans!");

        // 击中易拉罐声
        PlaySound(hitCansSound);

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