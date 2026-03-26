using UnityEngine;
using System.Collections;

/// <summary>
/// 小女孩移动控制器 - 适配 Level 2 动画、跟随逻辑与最终坐下逻辑
/// </summary>
public class LittleGirlController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public Vector3 moveDirection = Vector3.left;
    public bool autoStart = true;

    [Header("Animator Control")]
    [Tooltip("如果不拖拽，脚本会自动在子物体中寻找模型上的Animator")]
    public Animator animator;

    [Header("Level 2 - Cat Follow")]
    [Tooltip("Level 2里拖入猫的Transform")]
    public Transform catTransform;
    [Tooltip("女孩始终落后猫多少X距离")]
    public float followOffsetX = 2f;
    [Tooltip("是否开启跟随模式")]
    public bool followCatMode = false;

    [Header("Debug")]
    public bool testMode = false;

    // 内部状态
    private bool canMove = false;
    private int currentStopPointIndex = 0;
    private bool reachedFinalStop = false;

    // Level 2 路径点相关
    private Transform waypointTarget = null;
    private System.Action onArrivalCallback = null;
    private bool waypointMode = false;
    private const float waypointArrivalThreshold = 0.3f;

    void Start()
    {
        // 自动获取挂在模型子物体上的 Animator
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (autoStart) canMove = true;
    }

    void Update()
    {
        // 基础游戏状态检查
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

        // --- Level 2 核心：跟随猫模式 ---
        if (followCatMode && catTransform != null)
        {
            HandleFollowCat();
            return;
        }

        // --- Level 2 核心：指定点移动模式 ---
        if (waypointMode)
        {
            HandleWaypointMove();
        }
        else
        {
            // 基础移动逻辑 (兼顾 Level 1 或测试)
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

    // ─── Level 2 动画与状态控制逻辑 ─────────────────────────────────

    private void SetMovingAnim(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", moving);
        }
    }

    /// <summary>
    /// 强制坐下（对应 AlbumBox 调用）
    /// 包含先拾取、再坐下的完整流程
    /// </summary>
    public void SitDown()
    {
        canMove = false;
        waypointMode = false;
        followCatMode = false;
        SetMovingAnim(false);

        if (animator != null)
        {
            // 触发拾取动作，并启动坐下协程
            animator.SetTrigger("pickUp");
            StartCoroutine(SitRoutine());
        }
    }

    /// <summary>
    /// 延迟进入坐下状态的协程
    /// </summary>
    private IEnumerator SitRoutine()
    {
        // 等待拾取动作播放一瞬间，根据你的 pickUp 动画实际长度调整这个时间
        // 如果捡起动作需要1秒，这里就填 1.0f
        yield return new WaitForSeconds(0.8f);

        if (animator != null)
        {
            animator.SetBool("isSitting", true);
        }
        Debug.Log("[Girl] Character is now sitting.");
    }

    /// <summary>
    /// （备用）单纯只播放拾取动画
    /// </summary>
    public void PlayPickUp()
    {
        if (animator != null)
        {
            animator.SetTrigger("pickUp");
        }
    }

    // ─── Level 2 移动行为实现 ─────────────────────────────────────

    void HandleFollowCat()
    {
        // 计算目标位置（女孩在猫后方 followOffsetX 的位置）
        float targetX = catTransform.position.x + followOffsetX;
        float distX = Mathf.Abs(transform.position.x - targetX);

        // 如果距离超过阈值，则行走，否则待机
        if (distX > 0.3f)
        {
            float dir = targetX > transform.position.x ? 1f : -1f;
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
            transform.position = pos;

            FlipTowards(new Vector3(targetX, 0, 0));
            SetMovingAnim(true); // 激活行走动画
        }
        else
        {
            SetMovingAnim(false); // 切换回 Idle 动画
        }
    }

    public void StartMovingTo(Transform target, System.Action onArrival = null)
    {
        if (target == null) return;
        waypointTarget = target;
        onArrivalCallback = onArrival;
        waypointMode = true;
        followCatMode = false; // 开始走向指定点时，必须强制关闭跟随猫模式
        canMove = true;
        SetMovingAnim(true);
        FlipTowards(target.position);
    }

    void HandleWaypointMove()
    {
        if (waypointTarget == null || !canMove) return;

        Vector3 targetPos = new Vector3(waypointTarget.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        FlipTowards(targetPos);

        if (Vector3.Distance(transform.position, targetPos) <= waypointArrivalThreshold)
        {
            canMove = false;
            waypointMode = false;
            SetMovingAnim(false); // 到达点位，停止动画

            var cb = onArrivalCallback;
            onArrivalCallback = null;
            waypointTarget = null;
            cb?.Invoke(); // 执行到达后的回调（比如调用 SitDown）
        }
    }

    // ─── 基础辅助 ────────────────────────────────────────────────

    void FlipTowards(Vector3 target)
    {
        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    // 保留 Level 1 触发器逻辑以防万一
    void OnTriggerEnter(Collider other)
    {
        if (waypointMode || followCatMode) return;
        StopPoint stopPoint = other.GetComponent<StopPoint>();
        if (stopPoint != null && stopPoint.stopIndex == currentStopPointIndex)
        {
            canMove = false;
            currentStopPointIndex++;
            if (stopPoint.isFinalStop)
            {
                reachedFinalStop = true;
                GameManager.Instance?.GameWin();
            }
        }
    }
}