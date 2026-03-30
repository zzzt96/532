using UnityEngine;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance { get; private set; }

    public enum Phase
    {
        Idle, SkylightOpened, MirrorToDrawer, DrawerOpened,
        CatOnWardrobe, BalloonPhase, FanPhase, RockingChairPhase,
        LampPhase, Complete
    }

    [Header("Current Phase (Read Only)")]
    public Phase currentPhase = Phase.Idle;

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

    [Header("Girl Waypoints")]
    public Transform girlWaypoint1;
    public Transform girlWaypoint2;
    public Transform girlWaypointFinal;
    
    [Header("Scene Transition")]
    public string nextSceneName;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─── Helper：同时设置ToyBase和InteractableTag的canBePossessed ──
    void Unlock(ToyBase toy)
    {
        if (toy == null) return;
        toy.canBePossessed = true;
        Debug.Log($"[L2] Unlocked: {toy.gameObject.name}");
    }

    void Lock(ToyBase toy)
    {
        if (toy == null) return;
        toy.canBePossessed = false;
        var tag = toy.GetComponent<InteractableTag>();
        if (tag != null) tag.canBePossessed = false;
    }

    public void OnSkylightOpened()
    {
        if (currentPhase != Phase.Idle) return;
        currentPhase = Phase.SkylightOpened;
        Debug.Log("[L2] Phase: SkylightOpened");

        cat?.GoToSkylight();
        littleGirl?.StartMovingTo(girlWaypoint1);
        Unlock(mirror); 
    }

    public void OnMirrorAimedAtDrawer()
    {
        if (currentPhase != Phase.SkylightOpened) return;
        currentPhase = Phase.MirrorToDrawer;
        Debug.Log("[L2] Phase: MirrorToDrawer");

        cat?.GoToDrawer(drawer?.transform);
    }

    public void OnDrawerOpened()
    {
        Debug.Log($"[L2] OnDrawerOpened called, phase: {currentPhase}");
        if (currentPhase != Phase.MirrorToDrawer)
        {
            Debug.LogWarning($"[L2] Phase wrong! Expected MirrorToDrawer, got {currentPhase}");
            return;
        }
        currentPhase = Phase.DrawerOpened;
        Debug.Log("[L2] Phase: DrawerOpened");

        littleGirl?.StartMovingTo(girlWaypoint2);
    }

    /// <summary>镜子Zone2触发：猫从抽屉跳到柜顶</summary>
    public void OnMirrorAimedAtWardrobe()
    {
        if (currentPhase != Phase.DrawerOpened) return;
        currentPhase = Phase.CatOnWardrobe;
        Debug.Log("[L2] Phase: MirrorAimedAtWardrobe → Cat jumping to wardrobe");
        cat?.JumpToWardrobeTop();
    }

    /// <summary>猫跳上衣柜顶后调用（由CatNPC调用）</summary>
    public void OnCatOnWardrobe()
    {
        // OnMirrorAimedAtWardrobe已经把phase设为CatOnWardrobe了，这里直接检查
        if (currentPhase != Phase.CatOnWardrobe) return;
        Debug.Log("[L2] Phase: CatOnWardrobe → Unlocking balloon");

        Unlock(balloon);
    }

    public void OnBalloonTriggeredFan()
    {
        if (currentPhase != Phase.CatOnWardrobe) return;
        currentPhase = Phase.BalloonPhase;
        Debug.Log("[L2] Phase: BalloonPhase → Fan activated");

        fan?.TurnOn();
        Unlock(fan);
    }

    public void OnFanBlowTriggeredChair()
    {
        if (currentPhase != Phase.BalloonPhase) return;
        currentPhase = Phase.FanPhase;
        Debug.Log("[L2] Phase: FanPhase → RockingChair triggered");

        rockingChair?.StartRocking();
    }

    public void OnCatOnRockingChair()
    {
        if (currentPhase != Phase.FanPhase) return;
        currentPhase = Phase.RockingChairPhase;
        Debug.Log("[L2] Phase: RockingChairPhase → Unlocking lamp");

        Unlock(deskLamp);
    }

    public void OnCatOnAlbumBox()
    {
        if (currentPhase != Phase.RockingChairPhase) return;
        currentPhase = Phase.LampPhase;
        Debug.Log("[L2] Phase: LampPhase → Album falling");

        albumBox?.DropAlbum();
        littleGirl?.StartMovingTo(girlWaypointFinal, onArrival: OnLevelComplete);
    }
    
    public void OnLevelComplete()
    {
        currentPhase = Phase.Complete;
        Debug.Log("[L2] Level Complete!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}