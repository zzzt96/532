using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 小女孩移动控制器 - 绝对控制/定点移动版
/// 抛弃模糊的跟随逻辑，完全通过指定 Transform 点位来精确控制移动和动作
/// </summary>
public class LittleGirlController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Animator Control")]
    public Animator animator;

    [Header("Arrival Settings")]
    [Tooltip("到达目标点的精准度，0.01 几乎就是绝对重合")]
    public float arrivalThreshold = 0.01f;

    // 内部状态
    private bool isMovingToTarget = false;
    private Transform currentTarget = null;
    private Action onArrivalCallback = null;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying))
            return;

        // 核心移动逻辑：如果有目标点，就精确移动过去
        if (isMovingToTarget && currentTarget != null)
        {
            // 只在 X 和 Z 轴上移动，保持原本的高度 (Y轴)
            Vector3 targetPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);

            // 精确移动
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            FlipTowards(targetPos);

            // 检查是否精确到达
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= arrivalThreshold)
            {
                // 到达目标点，彻底停稳
                transform.position = targetPos;
                isMovingToTarget = false;
                currentTarget = null;
                SetMovingAnim(false);

                // 执行到达后的回调动作（比如拾取、坐下等）
                Action cb = onArrivalCallback;
                onArrivalCallback = null;
                cb?.Invoke();
            }
        }
    }

    // ─── 核心控制接口（供外部脚本调用） ──────────────────────────

    /// <summary>
    /// 指令：走到指定位置，并在到达后执行动作
    /// 用法示例：girl.GoToPosition(point1, () => girl.PlayPickUp());
    /// </summary>
    public void GoToPosition(Transform target, Action onArrival = null)
    {
        if (target == null) return;

        currentTarget = target;
        onArrivalCallback = onArrival;
        isMovingToTarget = true;

        SetMovingAnim(true);
        FlipTowards(target.position);
    }

    /// <summary>
    /// 指令：原地播放拾取动画
    /// </summary>
    public void PlayPickUp()
    {
        StopMovement(); // 确保停下
        if (animator != null) animator.SetTrigger("pickUp");
    }

    /// <summary>
    /// 指令：原地播放拾取，然后坐下
    /// </summary>
    public void PickUpAndSit()
    {
        StopMovement();
        if (animator != null)
        {
            animator.SetTrigger("pickUp");
            StartCoroutine(SitRoutine());
        }
    }

    /// <summary>
    /// 指令：直接坐下
    /// </summary>
    public void JustSit()
    {
        StopMovement();
        if (animator != null) animator.SetBool("isSitting", true);
    }

    /// <summary>
    /// 强制停止所有动作
    /// </summary>
    public void StopMovement()
    {
        isMovingToTarget = false;
        currentTarget = null;
        SetMovingAnim(false);
    }

    // ─── 内部辅助与动画 ──────────────────────────────────────────

    private IEnumerator SitRoutine()
    {
        // 等待拾取动作播放一小会儿，再切换到坐下（根据你的动画长度微调）
        yield return new WaitForSeconds(0.8f);
        if (animator != null) animator.SetBool("isSitting", true);
    }

    private void SetMovingAnim(bool moving)
    {
        if (animator != null) animator.SetBool("isMoving", moving);
    }

    private void FlipTowards(Vector3 target)
    {
        if (Mathf.Abs(target.x - transform.position.x) < 0.01f) return; // 距离太近不转身
        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }
}