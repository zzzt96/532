using UnityEngine;
using System.Collections;

public class CatNPC : MonoBehaviour
{
    public enum CatState { Idle, WalkToSound, JumpOnTable, OnTable, JumpToBall, WalkToBoard, Done }

    [Header("State")]
    public CatState currentState = CatState.Idle;

    [Header("References")]
    public Animator animator;
    public Transform tableJumpTarget;   // 右边桌面落点
    public Transform ballJumpTarget;    // 小球架子旁落点
    public Transform catLandTarget;     // 猫撞完球后的地面落点
    public Ball ball;
    public CarDropBoard carDropBoard;   // 拖入 CarDropBoard

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float arriveDistance = 0.8f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.6f;

    [Header("Timing")]
    public float waitOnTableDuration = 0.8f;

    private const string CLIP_IDLE = "Idle";
    private const string CLIP_WALK = "Walk";
    private const string CLIP_JUMP = "Jump";

    private Vector3 targetPos;
    private bool isMoving = false;

    void Start()
    {
        PlayAnim(CLIP_IDLE);
    }

    void Update()
    {
        if ((currentState == CatState.WalkToSound || currentState == CatState.WalkToBoard) && isMoving)
            MoveTo(targetPos);
    }

    // 水瓶落地触发
    public void AttractedBySound(Vector3 soundPos)
    {
        if (currentState != CatState.Idle) return;
        targetPos = new Vector3(soundPos.x, transform.position.y, transform.position.z);
        currentState = CatState.WalkToSound;
        isMoving = true;
        PlayAnim(CLIP_WALK);
        Debug.Log($"[Cat] Walking to bottle at X={soundPos.x:F2}");
    }

    // 植物晃动触发 - 走向木板
    public void AttractedByIvy(Vector3 boardPos)
    {
        if (currentState != CatState.Done) return;
        // 同时更新Z轴，让猫走到BoardPosition的Z位置
        targetPos = new Vector3(boardPos.x, transform.position.y, boardPos.z);
        currentState = CatState.WalkToBoard;
        isMoving = true;
        PlayAnim(CLIP_WALK);
    }

    void MoveTo(Vector3 target)
    {
        float distX = Mathf.Abs(target.x - transform.position.x);

        if (distX <= arriveDistance)
        {
            isMoving = false;

            if (currentState == CatState.WalkToSound)
            {
                if (tableJumpTarget != null)
                    StartCoroutine(JumpOnTableRoutine());
            }
            else if (currentState == CatState.WalkToBoard)
            {
                // 到达木板，触发翻倒
                PlayAnim(CLIP_IDLE);
                if (carDropBoard != null)
                    carDropBoard.TipBoard();
                currentState = CatState.Done;
                Debug.Log("[Cat] Reached board, tipping it!");
            }
            return;
        }

        float dir = target.x > transform.position.x ? 1f : -1f;
        float zDiff = targetPos.z - transform.position.z;
        float zStep = Mathf.MoveTowards(0, zDiff, walkSpeed * Time.deltaTime);
        transform.position += new Vector3(dir * walkSpeed * Time.deltaTime, 0, zStep);
        
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1f : -1f);
        transform.localScale = scale;
    }

    IEnumerator JumpOnTableRoutine()
    {
        currentState = CatState.JumpOnTable;
        PlayAnim(CLIP_JUMP);

        yield return StartCoroutine(DoJump(transform.position, tableJumpTarget.position));

        currentState = CatState.OnTable;
        PlayAnim(CLIP_IDLE);
        Debug.Log("[Cat] On table!");

        yield return new WaitForSeconds(waitOnTableDuration);

        if (ballJumpTarget != null)
            StartCoroutine(JumpToBallRoutine());
    }

    IEnumerator JumpToBallRoutine()
    {
        currentState = CatState.JumpToBall;
        PlayAnim(CLIP_JUMP);

        yield return StartCoroutine(DoJump(transform.position, ballJumpTarget.position));

        Debug.Log("[Cat] Hit ball!");

        if (ball != null)
            ball.KnockOffShelf();

        if (catLandTarget != null)
        {
            PlayAnim(CLIP_JUMP);
            yield return StartCoroutine(DoJump(transform.position, catLandTarget.position));
        }

        currentState = CatState.Done;
        PlayAnim(CLIP_IDLE);
        Debug.Log("[Cat] Phase 1 done, waiting for ivy.");
    }

    IEnumerator DoJump(Vector3 from, Vector3 to)
    {
        // 翻转朝向
        float dirX = to.x - from.x;
        if (Mathf.Abs(dirX) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dirX > 0 ? 1f : -1f);
            transform.localScale = scale;
        }

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            Vector3 pos = Vector3.Lerp(from, to, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = pos;
            yield return null;
        }
        transform.position = to;
    }

    void PlayAnim(string stateName)
    {
        if (animator == null) return;
        animator.CrossFade(stateName, 0.1f, 0);
    }
}