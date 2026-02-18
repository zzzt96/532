using UnityEngine;

public class ToyRocket : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Movement Bounds (X range on shelf)")]
    public float minX = -45f;
    public float maxX = -38f;

    [Header("State")]
    public bool isActivated = false;

    [Header("Fall Animation")]
    public float fallSpeed = 120f;
    public float fallTargetAngle = 90f;
    public bool fallLeft = true;
    private bool isFalling = false;
    private float currentFallAngle = 0f;

    [Header("Clock Detection")]
    public TableClock targetClock;
    public float clockHitDistance = 1.0f; // 调小了判定距离，需要靠得更近

    [Header("Audio")]
    public AudioClip hitClockSound; // 撞击闹钟音效

    private float lockedZ;
    private float lockedY;
    private bool hasHitClock = false;
    private float debugTimer = 0f;

    // ★ 新增：防止刚附身就秒触发的变量
    private float possessStartX;
    private bool hasMovedSincePossess = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        lockedZ = transform.position.z;
        lockedY = transform.position.y;
        maxX = transform.position.x + 1f;
    }

    public void Activate()
    {
        if (isActivated) return;
        isActivated = true;
        isFalling = true;
        canBePossessed = false;
        currentFallAngle = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("[ToyRocket] Starting to fall over...");
    }

    // ★ 重写附身方法：记录附身时的初始位置
    public override void Possess()
    {
        base.Possess();
        possessStartX = transform.position.x;
        hasMovedSincePossess = false;
    }

    // 封装撞击闹钟的方法
    void DoHitClock(TableClock clock)
    {
        if (clock != null && !hasHitClock)
        {
            hasHitClock = true;

            // 播放撞击音效
            if (audioSrc && hitClockSound) audioSrc.PlayOneShot(hitClockSound);

            clock.TriggerAlarm();
            Debug.Log("[ToyRocket] HIT CLOCK! Alarm triggered!");
        }
    }

    void Update()
    {
        if (isFalling)
        {
            float step = fallSpeed * Time.deltaTime;
            currentFallAngle += step;

            float direction = fallLeft ? 1f : -1f;
            transform.Rotate(0f, 0f, step * direction, Space.World);

            if (currentFallAngle >= fallTargetAngle)
            {
                isFalling = false;
                canBePossessed = true;
                lockedY = transform.position.y;

                Debug.Log($"[ToyRocket] Fell over! Now can possess. (Won't trigger clock until possessed)");
            }
            return;
        }

        // ★ 核心修复：检查玩家是否真的移动了火箭
        if (isPossessed && !hasMovedSincePossess)
        {
            if (Mathf.Abs(transform.position.x - possessStartX) > 0.1f)
            {
                hasMovedSincePossess = true; // 玩家确实操作了火箭
            }
        }

        // 只有被附身、且玩家确实移动了火箭之后，才检测距离
        if (isActivated && !hasHitClock && isPossessed && hasMovedSincePossess)
        {
            if (targetClock == null)
            {
                debugTimer += Time.deltaTime;
                if (debugTimer > 1f)
                {
                    debugTimer = 0f;
                    TableClock found = FindFirstObjectByType<TableClock>();
                    if (found != null)
                    {
                        targetClock = found;
                        Debug.Log("[ToyRocket] Auto-found TableClock: " + found.gameObject.name);
                    }
                }
                return;
            }

            float xDist = Mathf.Abs(transform.position.x - targetClock.transform.position.x);

            debugTimer += Time.deltaTime;
            if (debugTimer > 0.5f)
            {
                debugTimer = 0f;
                Debug.Log($"[ToyRocket] Rocket X={transform.position.x:F1}, Clock X={targetClock.transform.position.x:F1}, dist={xDist:F2}");
            }

            if (xDist < clockHitDistance)
            {
                DoHitClock(targetClock);
            }
        }
    }

    public override void ToyUpdate()
    {
        if (!isActivated || isFalling) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Vector3 pos = transform.position;
        pos.z = lockedZ;
        pos.y = lockedY;

        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move = 1f;
        if (Input.GetKey(KeyCode.D)) move = -1f;

        if (Mathf.Abs(move) > 0.01f)
        {
            pos.x += move * moveSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    // 增加物理碰撞检测作为双保险
    void OnCollisionEnter(Collision collision)
    {
        if (isPossessed && hasMovedSincePossess)
        {
            TableClock clock = collision.collider.GetComponent<TableClock>();
            if (clock == null) clock = collision.collider.GetComponentInParent<TableClock>();
            DoHitClock(clock);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPossessed && hasMovedSincePossess)
        {
            TableClock clock = other.GetComponent<TableClock>();
            if (clock == null) clock = other.GetComponentInParent<TableClock>();
            DoHitClock(clock);
        }
    }
}