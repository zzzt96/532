using UnityEngine;

/// <summary>
/// Level 1 用法：autoStart = true，场景里放 StopPoint 物体，
///              各交互脚本调用 UnlockMovement() 让她继续走
///
/// Level 2 用法：autoStart = false，由 Level2Manager 调用
///              StartMovingTo(target) 分阶段驱动她移动
///              到达后自动停下，可传入 onArrival 回调
/// </summary>
public class LittleGirlController : MonoBehaviour
{
    // ─── Inspector ─────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.left; // Level 1 方向移动
    public bool autoStart = true;                // Level 1 = true, Level 2 = false

    [Header("Animator (Optional)")]
    public Animator animator;

    [Header("Debug")]
    public bool testMode = false;

    // ─── 私有状态 ──────────────────────────────────────────────
    private bool canMove = false;

    // Level 1 StopPoint 相关
    private int currentStopPointIndex = 0;
    private bool reachedFinalStop = false;

    // Level 2 Waypoint 相关
    private Transform waypointTarget = null;
    private System.Action onArrivalCallback = null;
    private bool waypointMode = false;
    private const float waypointArrivalThreshold = 0.2f;

    // ════════════════════════════════════════════════════════════
    void Start()
    {
        if (autoStart) canMove = true;
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying)
                return;
        }

        if (reachedFinalStop)
        {
            SetMovingAnim(false);
            return;
        }

        if (waypointMode)
        {
            HandleWaypointMove();
        }
        else
        {
            // Level 1：方向移动
            if (testMode || canMove)
            {
                transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
                SetMovingAnim(true);
            }
            else
            {
                SetMovingAnim(false);
            }
        }
    }

    // ════════════════════════════════════════════════════════════
    // Level 2 接口
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// Level 2 专用：移动到目标点，到达后停下并触发回调
    /// </summary>
    public void StartMovingTo(Transform target, System.Action onArrival = null)
    {
        if (target == null) return;
        waypointTarget = target;
        onArrivalCallback = onArrival;
        waypointMode = true;
        canMove = true;
        SetMovingAnim(true);
        FlipTowards(target.position);
        Debug.Log($"[Girl] Moving to {target.name}");
    }

    void HandleWaypointMove()
    {
        if (waypointTarget == null || !canMove) return;

        // 只移动X轴（侧视角）
        Vector3 targetPos = new Vector3(waypointTarget.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        FlipTowards(targetPos);

        if (Vector3.Distance(transform.position, targetPos) <= waypointArrivalThreshold)
        {
            canMove = false;
            waypointMode = false;
            SetMovingAnim(false);
            Debug.Log($"[Girl] Arrived at {waypointTarget.name}");

            var cb = onArrivalCallback;
            onArrivalCallback = null;
            waypointTarget = null;
            cb?.Invoke();
        }
    }

    // ════════════════════════════════════════════════════════════
    // Level 1 接口（保持原有逻辑不变）
    // ════════════════════════════════════════════════════════════

    void OnTriggerEnter(Collider other)
    {
        if (waypointMode) return; // Level 2 模式下忽略 StopPoint

        StopPoint stopPoint = other.GetComponent<StopPoint>();
        if (stopPoint != null && stopPoint.stopIndex == currentStopPointIndex)
        {
            canMove = false;
            currentStopPointIndex++;
            Debug.Log($"[Girl] Reached stop point {stopPoint.stopIndex}");

            if (stopPoint.isFinalStop)
            {
                reachedFinalStop = true;
                GameManager.Instance?.GameWin();
            }
        }
    }

    /// <summary>解锁移动（Level 1 各交互脚本调用）</summary>
    public void UnlockMovement()
    {
        canMove = true;
        waypointMode = false;
        SetMovingAnim(true);
    }

    /// <summary>强制停止</summary>
    public void StopMovement()
    {
        canMove = false;
        SetMovingAnim(false);
    }

    /// <summary>播放坐下（Level 2 结尾调用）</summary>
    public void SitDown()
    {
        StopMovement();
        if (animator != null) animator.Play("Sit");
    }

    /// <summary>Level 1 觉醒版入口（兼容旧调用）</summary>
    public void WakeUpAndMove()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        canMove = true;
        SetMovingAnim(true);
        Debug.Log("[Girl] Woke up!");
    }

    // ════════════════════════════════════════════════════════════
    // 辅助
    // ════════════════════════════════════════════════════════════

    void FlipTowards(Vector3 target)
    {
        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    void SetMovingAnim(bool moving)
    {
        if (animator != null) animator.SetBool("isMoving", moving);
    }
}