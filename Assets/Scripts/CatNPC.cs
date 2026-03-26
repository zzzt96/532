using UnityEngine;
using System.Collections;

/// <summary>
/// 猫咪 NPC - Level 1 & Level 2 通用
/// Level 1：AttractedBySound() → 跳桌 → 跳Ball → 走到木板
/// Level 2：GoToSkylight() → GoToDrawer() → JumpToWardrobeTop()
///          → GoToRockingChair() → GoToAlbumBox()
/// 所有 Level 2 引用（wardrobeTopPosition等）留空则对应行为自动跳过
/// </summary>
public class CatNPC : MonoBehaviour
{
    // ─── 状态枚举（Level 1 + Level 2 合并）───────────────────────
    public enum CatState
    {
        // 通用
        Idle, Turning,
        // Level 1
        WalkToSound, JumpOnTable, OnTable, JumpToBall, WalkToBoard, Done,
        // Level 2
        WalkToSkylight, SitInLight,
        WalkToDrawer, OpenDrawer,
        JumpOnDrawer, JumpOnWardrobe, SitOnWardrobe,
        WalkToChair, JumpOnChair, SitOnChair,
        WalkToBox, JumpOnBox
    }

    [Header("State (Read Only)")]
    public CatState currentState = CatState.Idle;

    // ─── 动画（Legacy Animation，和Level1一致）────────────────────
    [Header("Animation")]
    public Animation catAnimation;
    public string clipIdle       = "Idle";
    public string clipIdleLand   = "Idle2";
    public string clipWalk       = "Walk";
    public string clipJump       = "Jump";
    public string clipTurn       = "Turn";
    public string clipFinalState = "FinalAction";

    // ─── 移动参数 ──────────────────────────────────────────────
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float arriveDistance = 0.8f;

    // ─── 跳跃参数 ──────────────────────────────────────────────
    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.6f;

    // ─── 转身参数 ──────────────────────────────────────────────
    [Header("Turning")]
    public float turnDuration = 0.5f;

    // ─── Level 1 专用引用 ──────────────────────────────────────
    [Header("Level 1 References")]
    public Transform tableJumpTarget;
    public Transform ballJumpTarget;
    public Transform catLandTarget;
    public Ball ball;
    public CarDropBoard carDropBoard;
    public MemoryEffect memoryEffect;
    public float waitOnTableDuration = 0.8f;

    // ─── Level 2 专用引用 ──────────────────────────────────────
    [Header("Level 2 References (leave empty in Level 1)")]
    public Transform skylightLightSpot;
    public Transform drawerTopPosition;     // 抽屉打开后的跳跃踏板
    public Transform wardrobeTopPosition;   // 衣柜顶部
    public Transform rockingChairPosition;  // 摇椅落点
    public Transform albumBoxPosition;      // 木箱落点
    public Drawer drawerRef;                // 抽屉脚本引用、[Header("Level 2 References")]
    public Transform tableEdgeGroundPos; 

    // ─── 私有状态 ──────────────────────────────────────────────
    private Vector3 targetPos;
    private bool isMoving = false;
    private float currentFaceAngle = 90f;
    private System.Action onArrivalCallback;

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
        switch (currentState)
        {
            case CatState.WalkToSound:
            case CatState.WalkToBoard:
            case CatState.WalkToSkylight:
            case CatState.WalkToDrawer:
            case CatState.WalkToChair:
            case CatState.WalkToBox:
                if (isMoving) MoveTo(targetPos);
                break;
        }
    }

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(0, currentFaceAngle, 0);
    }

    // ── Level 1 公开接口 ────────────────────────────────────────
    public void AttractedBySound(Vector3 soundPos)
    {
        if (currentState != CatState.Idle) return;
        targetPos = new Vector3(soundPos.x, transform.position.y, transform.position.z);
        CheckTurnAndMove(CatState.WalkToSound);
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
    
    // ── Level 2 公开接口（由 Level2Manager 调用）────────────────
    public void GoToSkylight()
    {
        if (skylightLightSpot == null) { Debug.LogWarning("[Cat] skylightLightSpot not assigned!"); return; }
        StartCoroutine(DoJump(
            transform.position, 
            skylightLightSpot.position, 
            CatState.JumpOnTable, 
            () => {
                currentState = CatState.SitInLight;
                PlayAnim(clipIdle);
                Debug.Log("[Cat] On desk in sunlight.");
            }
        ));
    }

    public void GoToDrawer(Transform drawerTransform = null)
    {
        if (drawerTopPosition == null) { Debug.LogWarning("[Cat] drawerTopPosition not assigned!"); return; }
        StartCoroutine(DoJump(transform.position, drawerTopPosition.position, CatState.JumpOnDrawer, () =>
        {
            currentState = CatState.SitInLight;
            PlayAnim(clipIdle);
            drawerRef?.OpenByWeight();
            Debug.Log("[Cat] Jumped onto drawer!");
        }));
    }

    public void JumpToWardrobeTop()
    {
        // 猫已经在抽屉上，直接跳柜顶
        StartCoroutine(DoJump(transform.position, SafePos(wardrobeTopPosition), CatState.JumpOnWardrobe, () =>
        {
            currentState = CatState.SitOnWardrobe;
            PlayAnim(clipIdle);
            Debug.Log("[Cat] On wardrobe top!");
            Level2Manager.Instance?.OnCatOnWardrobe();
        }));
    }
    
    /// <summary>Balloon触发：猫从柜顶跳到指定位置（风扇桌）</summary>
    public void JumpToPosition(Transform target)
    {
        if (target == null) return;
        StartCoroutine(DoJump(transform.position, target.position, CatState.JumpOnChair, () =>
        {
            currentState = CatState.SitOnChair;
            PlayAnim(clipIdle);
            Debug.Log("[Cat] Landed after balloon jump!");
        }));
    }

    public void GoToRockingChair()
    {
        if (rockingChairPosition == null) return;

        // 第一步：走到桌子边缘
        SetWalkTarget(
            new Vector3(tableEdgeGroundPos.position.x, transform.position.y, transform.position.z),
            CatState.WalkToChair,
            () => {
                // 第二步：跳下桌到地面
                StartCoroutine(DoJump(transform.position, tableEdgeGroundPos.position, CatState.JumpOnChair, () =>
                {
                    // 第三步：走到摇椅
                    SetWalkTarget(
                        new Vector3(rockingChairPosition.position.x, transform.position.y, transform.position.z),
                        CatState.WalkToChair,
                        () => {
                            currentState = CatState.SitOnChair;
                            PlayAnim(clipIdle);
                            Level2Manager.Instance?.OnCatOnRockingChair();
                        }
                    );
                }));
            }
        );
    }

    public void GoToAlbumBox()
    {
        if (albumBoxPosition == null) return;
        SetWalkTarget(
            new Vector3(albumBoxPosition.position.x, transform.position.y, transform.position.z),
            CatState.WalkToBox,
            () => StartCoroutine(DoJump(transform.position, SafePos(albumBoxPosition), CatState.JumpOnBox, () =>
            {
                currentState = CatState.Done;
                PlayAnim(clipIdleLand);
                Level2Manager.Instance?.OnCatOnAlbumBox();
            }))
        );
    }
    
    // ── 协程 ────────────────────────────────────────────────────
    IEnumerator OpenDrawerRoutine()
    {
        currentState = CatState.OpenDrawer;
        PlayAnim(clipIdle);
        Debug.Log("[Cat] Opening drawer...");

        drawerRef?.Open();

        yield return new WaitForSeconds(0.8f);
        // JumpToWardrobeTop 会在 Level2Manager.OnDrawerOpened 之后被调用
    }

    /// <summary>通用跳跃协程，跳完后调 onLand 回调</summary>
    IEnumerator DoJump(Vector3 from, Vector3 to, CatState jumpState, System.Action onLand)
    {
        currentState = jumpState;
        PlayAnim(clipJump);

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
        PlayAnim(clipIdle);
        onLand?.Invoke();
    }

    // Level 1 原有的具体跳跃序列
    IEnumerator JumpOnTableRoutine()
    {
        yield return StartCoroutine(DoJump(transform.position, tableJumpTarget.position, CatState.JumpOnTable, () => { }));
        currentState = CatState.OnTable;
        PlayAnim(clipIdle);
        yield return new WaitForSeconds(waitOnTableDuration);
        if (ballJumpTarget != null) StartCoroutine(JumpToBallRoutine());
    }

    IEnumerator JumpToBallRoutine()
    {
        yield return StartCoroutine(DoJump(transform.position, ballJumpTarget.position, CatState.JumpToBall, () => { }));
        ball?.KnockOffShelf();
        if (catLandTarget != null)
            yield return StartCoroutine(DoJump(transform.position, catLandTarget.position, CatState.Done, () => { }));
        currentState = CatState.Done;
        PlayAnim(clipIdleLand);
    }
    
    // ── 移动辅助 ─────────────────────────────────────────────────
    void SetWalkTarget(Vector3 pos, CatState state, System.Action onArrival)
    {
        targetPos = pos;
        onArrivalCallback = onArrival;
        CheckTurnAndMove(state);
    }

    void CheckTurnAndMove(CatState nextState)
    {
        float targetAngle = targetPos.x > transform.position.x ? 90f : -90f;
        if (Mathf.Abs(Mathf.DeltaAngle(currentFaceAngle, targetAngle)) > 10f)
            StartCoroutine(TurnAndWalkRoutine(nextState, targetAngle));
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
            currentFaceAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsed / turnDuration);
            yield return null;
        }
        currentFaceAngle = targetAngle;
        currentState = nextState;
        isMoving = true;
        PlayAnim(clipWalk);
    }

    void MoveTo(Vector3 target)
    {
        float distX = Mathf.Abs(target.x - transform.position.x);

        if (distX <= arriveDistance)
        {
            isMoving = false;

            // Level 1 到达逻辑
            if (currentState == CatState.WalkToSound)
            {
                if (tableJumpTarget != null) StartCoroutine(JumpOnTableRoutine());
            }
            else if (currentState == CatState.WalkToBoard)
            {
                PlayAnim(clipFinalState);
                carDropBoard?.TipBoard();
                memoryEffect?.ActivateEffect();
                currentState = CatState.Done;
            }
            // Level 2 到达：执行回调
            else
            {
                var cb = onArrivalCallback;
                onArrivalCallback = null;
                cb?.Invoke();
            }
            return;
        }

        float zDiff = target.z - transform.position.z;
        float zStep = Mathf.MoveTowards(0, zDiff, walkSpeed * Time.deltaTime);
        float dirX = target.x > transform.position.x ? 1f : -1f;
        transform.position += new Vector3(dirX * walkSpeed * Time.deltaTime, 0, zStep);
        currentFaceAngle = dirX > 0 ? 90f : -90f;
    }

    /// <summary>安全取Transform的position（null时返回原地）</summary>
    Vector3 SafePos(Transform t) => t != null ? t.position : transform.position;

    void PlayAnim(string stateName)
    {
        if (catAnimation == null) return;
        catAnimation.CrossFade(stateName, 0.2f);
    }
}