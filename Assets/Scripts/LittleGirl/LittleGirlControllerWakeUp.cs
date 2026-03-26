using UnityEngine;

public class LittleGirlControllerWakeUp : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool canMove = false;

    [Header("Movement Path")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    [Header("State")]
    public bool hasWokenUp = false;

    // ─── 新增：动画控制 ───
    [Header("Animator Control")]
    public Animator animator;

    void Start()
    {
        // 自动获取挂在模型子物体上的 Animator
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (!canMove || !hasWokenUp) return;

        MoveToNextWaypoint();
    }

    /// <summary>
    /// 醒来并开始移动
    /// </summary>
    public void WakeUpAndMove()
    {
        if (hasWokenUp) return;

        hasWokenUp = true;
        canMove = true;
        currentWaypointIndex = 0;

        // Capsule 从躺下变成站立
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // ─── 新增：触发站立并开始行走的动画 ───
        SetMovingAnim(true);

        Debug.Log("[LittleGirl] Woke up! Starting to move.");
    }

    void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachedEnd();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
        {
            currentWaypointIndex++;
            return;
        }

        // 【修复点】：直接使用目标点的完整坐标（包括高度），不再锁定Y轴
        Vector3 targetPos = target.position;

        // 使用 MoveTowards 平滑移动，比算 direction 更稳
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 翻转朝向（只看X轴的相对位置）
        FlipTowards(targetPos);

        // 检查是否到达（缩小判定范围，让她完全走到点上）
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            Debug.Log($"[LittleGirl] Reached waypoint {currentWaypointIndex}");
            currentWaypointIndex++;
        }
    }

    void ReachedEnd()
    {
        canMove = false;

        // ─── 新增：到达终点，停下行走动画 ───
        SetMovingAnim(false);

        Debug.Log("[LittleGirl] Reached the door!");

        // 通知 GameManager 游戏完成
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameComplete();
        }
    }

    // ─── 新增：动画与辅助方法 ───
    private void SetMovingAnim(bool moving)
    {
        if (animator != null)
        {
            // 依赖 Animator 中配置好的 "isMoving" 布尔值
            animator.SetBool("isMoving", moving);
        }
    }

    private void FlipTowards(Vector3 target)
    {
        // 距离太近就不翻转，防止到达目标点时鬼畜抽搐
        if (Mathf.Abs(target.x - transform.position.x) < 0.01f) return;

        // 根据目标 X 坐标和当前 X 坐标对比，决定朝左还是朝右
        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }
}