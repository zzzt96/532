using UnityEngine;

public class LittleGirlController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.left; // 移动方向（默认向左）
    public bool autoStart = true; // 游戏开始时是否自动移动

    [Header("Animator (Optional)")]
    public Animator animator;

    [Header("Debug")]
    public bool testMode = false; // 测试模式：跳过所有检查直接往左走

    // 内部状态
    private bool canMove = false;
    private int currentStopPointIndex = 0; // 当前到达第几个停止点
    private bool reachedFinalStop = false;

    void Start()
    {
        if (autoStart)
            canMove = true;
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
            StopMoving();
            return;
        }

        if (testMode || canMove)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
            StartMoving();
        }
        else
        {
            StopMoving();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 撞到停止点
        StopPoint stopPoint = other.GetComponent<StopPoint>();
        if (stopPoint != null && stopPoint.stopIndex == currentStopPointIndex)
        {
            canMove = false; // 停下来
            currentStopPointIndex++; // 准备去下一个停止点

            Debug.Log($"Reached stop point {stopPoint.stopIndex}");

            // 如果是最后一个停止点，游戏结束
            if (stopPoint.isFinalStop)
            {
                reachedFinalStop = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.GameWin();
            }
        }
    }

    /// <summary>
    /// 外部调用：解锁，让小女孩继续往前走
    /// 从任何交互脚本（电灯、书本、篮球等）调用这个方法
    /// </summary>
    public void UnlockMovement()
    {
        canMove = true;
    }

    /// <summary>
    /// 外部调用：停止移动
    /// </summary>
    public void StopMovement()
    {
        canMove = false;
    }

    void StartMoving()
    {
        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    void StopMoving()
    {
        if (animator != null)
            animator.SetBool("isMoving", false);
    }
}