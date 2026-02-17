using UnityEngine;

public class BunnyToy : ToyBase
{
    public enum BunnyState { Inactive, Swinging, OnCurtainRope, Dropped }

    [Header("State")]
    public BunnyState currentState = BunnyState.Inactive;

    // ========== 1. 摆动设置 ==========
    [Header("1. Swing Settings")]
    public Transform ropePivot;
    public float swingSpeed = 2f;
    public float leftMaxAngle = 40f;
    public float rightMaxAngle = -40f;

    private float swingTimer = 0f;
    private float currentAngle = 0f;

    // ========== 2. 窗帘 ==========
    [Header("2. Curtain Settings")]
    public Transform curtainRopePosition;
    public float pullSpeed = 2f;
    public float curtainMinY = -3f;
    public Curtain curtain;

    // ========== 3. 掉落 ==========
    [Header("3. Drop Settings")]
    public Transform boardPosition;
    public float dropThreshold = 1.5f;
    public float bunnyDropForce = 6f;       // 兔子砸木板的力度（比水杯的12小）
    public CarDropBoard carDropBoard;       // ★ 直接拖入 CarDropBoard

    private float ropeStartY;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
    }

    public void Activate()
    {
        if (currentState != BunnyState.Inactive) return;

        currentState = BunnyState.Swinging;
        canBePossessed = true;
        swingTimer = 0f;

        Debug.Log("[BunnyToy] Activated! Swinging. Possess me and press Space to jump!");
    }

    void Update()
    {
        if (currentState == BunnyState.Swinging && ropePivot != null)
        {
            swingTimer += Time.deltaTime * swingSpeed;
            float sinVal = Mathf.Sin(swingTimer);
            float t = (sinVal + 1f) * 0.5f;
            currentAngle = Mathf.Lerp(rightMaxAngle, leftMaxAngle, t);
            ropePivot.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }

    public override void ToyUpdate()
    {
        switch (currentState)
        {
            case BunnyState.Swinging:
                HandleSwinging();
                break;
            case BunnyState.OnCurtainRope:
                HandleOnCurtainRope();
                break;
        }
    }

    // ========== 摆动阶段 ==========
    void HandleSwinging()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpToCurtainRope();
        }
    }

    void JumpToCurtainRope()
    {
        currentState = BunnyState.OnCurtainRope;

        transform.SetParent(null);

        if (curtainRopePosition != null)
        {
            transform.position = curtainRopePosition.position;
            ropeStartY = curtainRopePosition.position.y;
        }

        transform.rotation = Quaternion.identity;

        if (ropePivot != null)
        {
            ropePivot.localRotation = Quaternion.identity;
        }

        // ★ 窗帘保持关闭状态（确保不会瞬间打开）
        if (curtain != null)
        {
            curtain.SetOpenProgress(0f);
        }

        Debug.Log("[BunnyToy] On curtain rope! S=pull down(open curtain), W=up, Space=drop when low enough.");
    }

    // ========== 窗帘拉绳阶段 ==========
    void HandleOnCurtainRope()
    {
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) moveY = 1f;
        if (Input.GetKey(KeyCode.S)) moveY = -1f;

        if (Mathf.Abs(moveY) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y += moveY * pullSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, ropeStartY + curtainMinY, ropeStartY + 0.5f);
            transform.position = pos;

            // 兔子越往下拉，窗帘越打开
            if (curtain != null)
            {
                float progress = Mathf.InverseLerp(ropeStartY, ropeStartY + curtainMinY, pos.y);
                curtain.SetOpenProgress(progress);
            }
        }

        // Space 掉落
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float distBelow = ropeStartY - transform.position.y;
            if (distBelow >= dropThreshold)
            {
                DropToBoard();
            }
            else
            {
                Debug.Log($"[BunnyToy] Pull down more! dist={distBelow:F1}, need>={dropThreshold}");
            }
        }
    }
    
    void DropToBoard()
    {
        currentState = BunnyState.Dropped;
        canBePossessed = false;

        if (boardPosition != null)
        {
            transform.position = boardPosition.position;
        }

        Debug.Log("[BunnyToy] Dropped to board!");

        // ★ 直接触发小车掉落（不需要碰撞检测）
        if (carDropBoard != null)
        {
            carDropBoard.TriggerDrop(bunnyDropForce);
            Debug.Log($"[BunnyToy] Triggered CarDropBoard with force={bunnyDropForce}!");
        }
        else
        {
            Debug.LogWarning("[BunnyToy] carDropBoard is null! Drag CarDropBoard into Inspector.");
        }
    }
}