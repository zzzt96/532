using UnityEngine;
using System.Collections;

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

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("行走脚步声 (建议勾选 Loop, 提供可循环短 wav)。Level 2 全程跟随猫和走向相册都用这一个声音。")]
    public SoundSlot walkingSound;
    // ===============================================

    // 内部状态
    private bool canMove = false;
    private bool reachedFinalStop = false;

    // Level 2 路径点相关
    private Transform waypointTarget = null;
    private System.Action onArrivalCallback = null;
    private bool waypointMode = false;
    private const float waypointArrivalThreshold = 0.3f;

    // 音频
    private AudioSource audioSrc;
    private bool isWalkingSoundPlaying = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        audioSrc = GetComponent<AudioSource>();

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

        // --- Level 2 核心：跟随猫模式 ---
        if (followCatMode && catTransform != null)
        {
            HandleFollowCat();
            return;
        }

        // --- Level 2 核心：走向指定点模式 ---
        if (waypointMode)
        {
            HandleWaypointMove();
        }
        else
        {
            // 基础移动逻辑 (兼容测试模式)
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

    // ─── 动画 + 音效统一控制 ─────────────────────────────────
    // SetMovingAnim 现在同时控制行走动画 + 脚步音效
    // 这样 3 种移动场景 (基础移动/跟随猫/走向指定点) 全部自动获得脚步声

    private void SetMovingAnim(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", moving);
        }

        // 同步控制脚步声
        if (moving)
            StartWalkingSound();
        else
            StopWalkingSound();
    }

    /// <summary>
    /// 强制坐下（保留方法供未来扩展使用，目前 Level 2 终点不调用此方法）
    /// </summary>
    public void SitDown()
    {
        canMove = false;
        waypointMode = false;
        followCatMode = false;
        SetMovingAnim(false);

        if (animator != null)
        {
            animator.SetTrigger("pickUp");
            StartCoroutine(SitRoutine());
        }
    }

    private IEnumerator SitRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        if (animator != null)
        {
            animator.SetBool("isSitting", true);
        }
        Debug.Log("[Girl] Character is now sitting.");
    }

    /// <summary>
    /// 停止所有移动，并播放弯腰捡起动画
    /// (保留方法供未来扩展使用，目前 Level 2 终点不调用此方法)
    /// </summary>
    public void PlayPickUp()
    {
        canMove = false;
        waypointMode = false;
        followCatMode = false;

        SetMovingAnim(false);

        if (animator != null)
        {
            animator.SetTrigger("pickUp");
        }
    }

    // ─── Level 2 移动行为实现 ─────────────────────────────────────

    void HandleFollowCat()
    {
        float side = transform.position.x > catTransform.position.x ? 1f : -1f;
        float targetX = catTransform.position.x + (side * Mathf.Abs(followOffsetX));
        float distX = Mathf.Abs(transform.position.x - targetX);

        if (distX > 0.05f)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
            transform.position = pos;

            FlipTowards(catTransform.position);
            SetMovingAnim(true);
        }
        else
        {
            FlipTowards(catTransform.position);
            SetMovingAnim(false);
        }
    }

    public void StartMovingTo(Transform target, System.Action onArrival = null)
    {
        if (target == null) return;
        waypointTarget = target;
        onArrivalCallback = onArrival;
        waypointMode = true;
        followCatMode = false;
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
            SetMovingAnim(false);

            var cb = onArrivalCallback;
            onArrivalCallback = null;
            waypointTarget = null;
            cb?.Invoke();
        }
    }
    
    void FlipTowards(Vector3 target)
    {
        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    // ==================== Audio Methods ====================

    void StartWalkingSound()
    {
        if (walkingSound == null || walkingSound.clip == null) return;
        if (audioSrc == null) return;
        if (isWalkingSoundPlaying) return;  // 防抖: 已经在播就不重启

        audioSrc.clip = walkingSound.clip;
        audioSrc.volume = walkingSound.volume;
        audioSrc.pitch = walkingSound.pitch +
            Random.Range(-walkingSound.randomPitchRange, walkingSound.randomPitchRange);
        audioSrc.loop = true;
        audioSrc.Play();
        isWalkingSoundPlaying = true;
    }

    void StopWalkingSound()
    {
        if (audioSrc == null) return;
        if (!isWalkingSoundPlaying) return;

        audioSrc.Stop();
        audioSrc.loop = false;
        isWalkingSoundPlaying = false;
    }
}