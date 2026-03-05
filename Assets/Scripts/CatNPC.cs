using UnityEngine;
using System.Collections;

public class CatNPC : MonoBehaviour
{
    public enum CatState { Idle, WalkToSound, JumpOnTable, OnTable, JumpToBall, WalkToBoard, Done, Turning }

    [Header("State")]
    public CatState currentState = CatState.Idle;

    [Header("References")]
    public Animation catAnimation;
    public Transform tableJumpTarget;
    public Transform ballJumpTarget;
    public Transform catLandTarget;
    public Ball ball;
    public CarDropBoard carDropBoard;

    // 【新增】用来触发特效的引用
    [Header("Events")]
    public MemoryEffect memoryEffect;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float arriveDistance = 0.8f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.6f;

    [Header("Timing")]
    public float waitOnTableDuration = 0.8f;
    public float turnDuration = 0.5f;

    [Header("Animation Clip Names")]
    public string clipIdle = "Idle";
    public string clipIdleLand = "Idle2";
    public string clipWalk = "Walk";
    public string clipJump = "Jump";
    public string clipTurn = "Turn";
    public string clipFinalState = "FinalAction";

    private Vector3 targetPos;
    private bool isMoving = false;
    private float currentFaceAngle = 90f;

    void Start()
    {
        currentFaceAngle = transform.eulerAngles.y;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;
        PlayAnim(clipIdle);
    }

    void Update()
    {
        if ((currentState == CatState.WalkToSound || currentState == CatState.WalkToBoard) && isMoving)
            MoveTo(targetPos);
    }

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(0, currentFaceAngle, 0);
    }

    public void AttractedBySound(Vector3 soundPos)
    {
        if (currentState != CatState.Idle) return;
        targetPos = new Vector3(soundPos.x, transform.position.y, transform.position.z);
        CheckTurnAndMove(CatState.WalkToSound);
    }

    void CheckTurnAndMove(CatState nextState)
    {
        float targetAngle = targetPos.x > transform.position.x ? 90f : -90f;

        if (Mathf.Abs(Mathf.DeltaAngle(currentFaceAngle, targetAngle)) > 10f)
        {
            StartCoroutine(TurnAndWalkRoutine(nextState, targetAngle));
        }
        else
        {
            currentState = nextState;
            isMoving = true;
            PlayAnim(clipWalk);
        }
    }

    IEnumerator TurnAndWalkRoutine(CatState nextState, float targetAngle)
    {
        currentState = CatState.Turning;
        isMoving = false;
        PlayAnim(clipTurn);

        float startAngle = currentFaceAngle;
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnDuration;
            currentFaceAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            yield return null;
        }

        currentFaceAngle = targetAngle;
        currentState = nextState;
        isMoving = true;
        PlayAnim(clipWalk);
    }

    public void AttractedByIvy(Vector3 boardPos)
    {
        if (currentState != CatState.Done) return;
        targetPos = new Vector3(boardPos.x, transform.position.y, boardPos.z);
        float targetAngle = targetPos.x > transform.position.x ? 90f : -90f;
        currentFaceAngle = targetAngle;
        currentState = CatState.WalkToBoard;
        isMoving = true;
        PlayAnim(clipWalk);
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
                PlayAnim(clipFinalState);
                if (carDropBoard != null)
                    carDropBoard.TipBoard();

                // 【新增核心逻辑】：到达目的地，触发回忆/高光特效！
                if (memoryEffect != null)
                    memoryEffect.ActivateEffect();

                currentState = CatState.Done;
            }
            return;
        }

        float zDiff = targetPos.z - transform.position.z;
        float zStep = Mathf.MoveTowards(0, zDiff, walkSpeed * Time.deltaTime);
        float dirX = target.x > transform.position.x ? 1f : -1f;
        transform.position += new Vector3(dirX * walkSpeed * Time.deltaTime, 0, zStep);
        currentFaceAngle = dirX > 0 ? 90f : -90f;
    }

    IEnumerator JumpOnTableRoutine()
    {
        currentState = CatState.JumpOnTable;
        PlayAnim(clipJump);
        yield return StartCoroutine(DoJump(transform.position, tableJumpTarget.position));
        currentState = CatState.OnTable;
        PlayAnim(clipIdle);
        yield return new WaitForSeconds(waitOnTableDuration);
        if (ballJumpTarget != null) StartCoroutine(JumpToBallRoutine());
    }

    IEnumerator JumpToBallRoutine()
    {
        currentState = CatState.JumpToBall;
        PlayAnim(clipJump);
        yield return StartCoroutine(DoJump(transform.position, ballJumpTarget.position));
        if (ball != null) ball.KnockOffShelf();
        if (catLandTarget != null)
        {
            PlayAnim(clipJump);
            yield return StartCoroutine(DoJump(transform.position, catLandTarget.position));
        }
        currentState = CatState.Done;
        PlayAnim(clipIdleLand);
    }

    IEnumerator DoJump(Vector3 from, Vector3 to)
    {
        float dirX = to.x - from.x;
        if (Mathf.Abs(dirX) > 0.01f)
            currentFaceAngle = dirX > 0 ? 90f : -90f;

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
        if (catAnimation == null) return;
        catAnimation.CrossFade(stateName, 0.2f);
    }
}