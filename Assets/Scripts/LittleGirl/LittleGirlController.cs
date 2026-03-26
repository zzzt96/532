using UnityEngine;

/// <summary>
/// 小女孩移动控制器 - Level 1 & Level 2 通用
/// Level 2 新增：followCat模式，永远比猫慢一步跟过去
/// </summary>
public class LittleGirlController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public Vector3 moveDirection = Vector3.left;
    public bool autoStart = true;

    [Header("Animator (Optional)")]
    public Animator animator;

    [Header("Level 2 - Cat Follow")]
    [Tooltip("Level 2里拖入猫的Transform，开启跟随模式")]
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

    // Level 2 waypoint
    private Transform waypointTarget = null;
    private System.Action onArrivalCallback = null;
    private bool waypointMode = false;
    private const float waypointArrivalThreshold = 0.3f;

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

        // 跟随猫模式（Level 2）
        if (followCatMode && catTransform != null)
        {
            HandleFollowCat();
            return;
        }

        if (waypointMode)
        {
            HandleWaypointMove();
        }
        else
        {
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

    // ─── Level 2：跟随猫 ────────────────────────────────────────
    void HandleFollowCat()
    {
        // 目标X = 猫X + offset（女孩永远在猫右边/后面一点）
        float targetX = catTransform.position.x + followOffsetX;
        float distX = Mathf.Abs(transform.position.x - targetX);

        if (distX > 0.3f)
        {
            float dir = targetX > transform.position.x ? 1f : -1f;
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
            transform.position = pos;
            FlipTowards(new Vector3(targetX, 0, 0));
            SetMovingAnim(true);
        }
        else
        {
            SetMovingAnim(false);
        }
    }

    // ─── Level 2：走到指定目标点 ────────────────────────────────
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

    // ─── Level 1 接口 ────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (waypointMode || followCatMode) return;

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

    public void UnlockMovement()
    {
        canMove = true;
        waypointMode = false;
        SetMovingAnim(true);
    }

    public void StopMovement()
    {
        canMove = false;
        SetMovingAnim(false);
    }

    public void SitDown()
    {
        StopMovement();
        if (animator != null) animator.Play("Sit");
    }

    public void WakeUpAndMove()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        canMove = true;
        SetMovingAnim(true);
        Debug.Log("[Girl] Woke up!");
    }

    // ─── 辅助 ────────────────────────────────────────────────────
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