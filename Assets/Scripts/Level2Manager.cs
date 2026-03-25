using UnityEngine;

/// <summary>
/// Level 2 总流程状态机
/// 控制所有谜题阶段的推进，其他脚本通过 Level2Manager.Instance 汇报进度
/// </summary>
public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance { get; private set; }

    // ─── 关卡阶段 ──────────────────────────────────────────────
    public enum Phase
    {
        Idle,               // 等待玩家开启天窗
        SkylightOpened,     // 天窗打开 → 猫走到光下，女孩开始移动
        MirrorToDrawer,     // 玩家附身镜子，把光打到抽屉
        DrawerOpened,       // 猫打开抽屉，光自动转向衣柜顶部
        CatOnWardrobe,      // 猫跳上衣柜顶
        BalloonPhase,       // 玩家附身气球晃动
        FanPhase,           // 玩家附身风扇吹东西
        RockingChairPhase,  // 摇椅晃动，猫过来
        LampPhase,          // 玩家附身台灯，导引猫跳木箱
        Complete            // 关卡结束
    }

    [Header("Current Phase (Read Only)")]
    public Phase currentPhase = Phase.Idle;

    // ─── 场景引用 ──────────────────────────────────────────────
    [Header("Scene References")]
    public CatNPC cat;
    public LittleGirlController littleGirl;
    public Mirror mirror;
    public Drawer drawer;
    public Balloon balloon;
    public Fan fan;
    public RockingChair rockingChair;
    public DeskLamp deskLamp;
    public AlbumBox albumBox;

    // ─── 小女孩移动阶段目标点 ──────────────────────────────────
    [Header("Girl Waypoints")]
    [Tooltip("天窗打开后女孩走到的位置")]
    public Transform girlWaypoint1; // 走到抽屉旁边
    [Tooltip("猫打开抽屉后女孩继续前进到的位置")]
    public Transform girlWaypoint2; // 走到房间中段
    [Tooltip("相册掉落后女孩最终到达的位置")]
    public Transform girlWaypointFinal; // 走到相册旁边坐下

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ═══════════════════════════════════════════════════════════
    // 外部脚本调用的事件通知接口（每一步完成后调用）
    // ═══════════════════════════════════════════════════════════

    /// <summary>天窗打开后调用</summary>
    public void OnSkylightOpened()
    {
        if (currentPhase != Phase.Idle) return;
        currentPhase = Phase.SkylightOpened;
        Debug.Log("[L2] Phase: SkylightOpened");

        // 猫：走向天窗光斑
        cat.GoToSkylight();
        // 女孩：开始自动向前移动（先走到 Waypoint1 停下等待）
        littleGirl.StartMovingTo(girlWaypoint1);
    }

    /// <summary>玩家把镜子光打到抽屉区域后调用（由Mirror.cs调用）</summary>
    public void OnMirrorAimedAtDrawer()
    {
        if (currentPhase != Phase.SkylightOpened) return;
        currentPhase = Phase.MirrorToDrawer;
        Debug.Log("[L2] Phase: MirrorToDrawer");

        // 猫：被镜子光吸引，走过去打开抽屉
        cat.GoToDrawer(drawer.transform);
    }

    /// <summary>猫打开抽屉后调用（由Drawer.cs调用）</summary>
    public void OnDrawerOpened()
    {
        if (currentPhase != Phase.MirrorToDrawer) return;
        currentPhase = Phase.DrawerOpened;
        Debug.Log("[L2] Phase: DrawerOpened");

        // 光线自动转向衣柜顶部（Mirror脚本内部的自动旋转）
        mirror.AutoRedirectToWardrobe();
        // 女孩继续向前走到 Waypoint2
        littleGirl.StartMovingTo(girlWaypoint2);
        // 猫：借助抽屉跳到衣柜顶部
        cat.JumpToWardrobeTop();
    }

    /// <summary>猫成功跳上衣柜后调用（由CatNPC.cs调用）</summary>
    public void OnCatOnWardrobe()
    {
        if (currentPhase != Phase.DrawerOpened) return;
        currentPhase = Phase.CatOnWardrobe;
        Debug.Log("[L2] Phase: CatOnWardrobe");

        // 解锁气球可附身
        balloon.canBePossessed = true;
    }

    /// <summary>气球被猫拍打，挂着的东西落在风扇开关上（由Balloon.cs调用）</summary>
    public void OnBalloonTriggeredFan()
    {
        if (currentPhase != Phase.CatOnWardrobe) return;
        currentPhase = Phase.BalloonPhase;
        Debug.Log("[L2] Phase: BalloonPhase → Fan activated");

        // 风扇自动启动（开关被触发）
        fan.TurnOn();
        // 解锁风扇可附身
        fan.canBePossessed = true;
    }

    /// <summary>风扇吹倒东西，撞到摇椅（由Fan.cs调用）</summary>
    public void OnFanBlowTriggeredChair()
    {
        if (currentPhase != Phase.BalloonPhase) return;
        currentPhase = Phase.FanPhase;
        Debug.Log("[L2] Phase: FanPhase → RockingChair triggered");

        rockingChair.StartRocking();
    }

    /// <summary>摇椅晃动吸引猫跳上来（由RockingChair.cs调用）</summary>
    public void OnCatOnRockingChair()
    {
        if (currentPhase != Phase.FanPhase) return;
        currentPhase = Phase.RockingChairPhase;
        Debug.Log("[L2] Phase: RockingChairPhase");

        // 解锁台灯可附身
        deskLamp.canBePossessed = true;
    }

    /// <summary>台灯光打到木箱区域，猫跳上木箱（由DeskLamp.cs调用）</summary>
    public void OnCatOnAlbumBox()
    {
        if (currentPhase != Phase.RockingChairPhase) return;
        currentPhase = Phase.LampPhase;
        Debug.Log("[L2] Phase: LampPhase → Album falling");

        // 相册掉落
        albumBox.DropAlbum();
        // 女孩走到最终位置拿相册
        littleGirl.StartMovingTo(girlWaypointFinal, onArrival: OnLevelComplete);
    }

    void OnLevelComplete()
    {
        currentPhase = Phase.Complete;
        Debug.Log("[L2] Level Complete!");
        // TODO: 触发过场/切换场景
    }
}