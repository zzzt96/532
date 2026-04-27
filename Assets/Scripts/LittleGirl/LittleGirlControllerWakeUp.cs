using UnityEngine;
using System.Collections;

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

    [Header("Animator Control")]
    public Animator animator;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("起床声 (布料摩擦, 醒来瞬间播放)")]
    public SoundSlot clothRustleSound;

    [Tooltip("下床声 (脚步落地, 起床后短暂延迟播放)")]
    public SoundSlot stepDownSound;

    [Tooltip("下床声延迟 (秒, 起床后多久触发下床声)")]
    public float stepDownDelay = 0.5f;

    [Tooltip("行走脚步声 (建议勾选 Loop, 提供可循环短 wav)")]
    public SoundSlot walkingSound;
    // ===============================================

    private AudioSource audioSrc;
    private bool isWalkingSoundPlaying = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!canMove || !hasWokenUp) return;

        MoveToNextWaypoint();
    }

    public void WakeUpAndMove()
    {
        if (hasWokenUp) return;

        hasWokenUp = true;
        canMove = true;
        currentWaypointIndex = 0;

        transform.rotation = Quaternion.Euler(0, 0, 0);

        SetMovingAnim(true);

        // 启动音效序列: 起床 -> (延迟) -> 下床 -> 行走 loop
        StartCoroutine(PlayWakeUpSequence());

        Debug.Log("[LittleGirl] Woke up! Starting to move.");
    }

    IEnumerator PlayWakeUpSequence()
    {
        PlayOneShotSlot(clothRustleSound);

        yield return new WaitForSeconds(stepDownDelay);
        PlayOneShotSlot(stepDownSound);

        StartWalkingSound();
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

        Vector3 targetPos = target.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        FlipTowards(targetPos);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            Debug.Log($"[LittleGirl] Reached waypoint {currentWaypointIndex}");
            currentWaypointIndex++;
        }
    }

    void ReachedEnd()
    {
        canMove = false;

        SetMovingAnim(false);

        // 停止行走 loop
        StopWalkingSound();

        Debug.Log("[LittleGirl] Reached the door!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameComplete();
        }
    }

    private void SetMovingAnim(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", moving);
        }
    }

    private void FlipTowards(Vector3 target)
    {
        if (Mathf.Abs(target.x - transform.position.x) < 0.01f) return;

        float dir = target.x > transform.position.x ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    // ==================== Audio Methods ====================

    void PlayOneShotSlot(SoundSlot slot)
    {
        if (slot == null || slot.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.pitch = slot.pitch +
            Random.Range(-slot.randomPitchRange, slot.randomPitchRange);
        audioSrc.PlayOneShot(slot.clip, slot.volume);
    }

    void StartWalkingSound()
    {
        if (walkingSound == null || walkingSound.clip == null) return;
        if (audioSrc == null) return;
        if (isWalkingSoundPlaying) return;

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