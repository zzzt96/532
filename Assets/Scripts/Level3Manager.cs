using UnityEngine;

public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }

    public enum Phase
    {
        Idle,
        CansCleared,
        LightsOn,
        LampSwung,
        PlankFell,
        AppleDone,
        DeskLampOn,
        MusicPlayed,
        Complete
    }

    [Header("Current Phase (Read Only)")]
    public Phase currentPhase = Phase.Idle;

    [Header("Scene References")]
    public SpringGlove springGlove;
    public Cans cans;
    public Switch switchObj;
    public PendantLight pendantLight;
    public TableClock3 tableClock;
    public ComputerTable computerTable;
    public Blank blank;
    public Apple apple;
    public AirPump airPump;
    public BalloonL3 balloonL3;
    public MusicBox musicBox;
    public GiftBox giftBox;

    [Header("NPC References")]
    public LittleGirlController littleGirl;
    public CatNPC cat;

    [Header("Waypoints")]
    public Transform girlWaypointRight;
    public Transform girlWaypointLeft;
    public Transform catWaypointLeft;
    public Transform girlWaypointOnPlank;
    public Transform catWaypointOnPlank;
    public Transform girlWaypointAtPump;
    public Transform catWaypointAtPump;
    public Transform catWaypointOnDesk;

    [Tooltip("礼物出现后小女孩走过来的最终位置")]
    public Transform girlWaypointFinal; 
    
    [Header("Light References")]
    public Light[] sceneLights;
    public float litIntensity = 100f;

    [Header("Scene Transition")]
    public string nextSceneName; 
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Unlock(springGlove);
        Lock(switchObj);
        Lock(pendantLight);
        Lock(tableClock);
        Lock(apple);
        Lock(airPump);
        Lock(musicBox);
        SetLights(false);
    }

    // ─── Helpers ────────────────────────────────────────────────
    void Unlock(ToyBase toy)
    {
        if (toy == null) return;
        toy.canBePossessed = true;
        var tag = toy.GetComponent<InteractableTag>();
        if (tag != null) tag.canBePossessed = true;
        Debug.Log($"[L3] Unlocked: {toy.gameObject.name}");
    }

    void Lock(ToyBase toy)
    {
        if (toy == null) return;
        toy.canBePossessed = false;
        var tag = toy.GetComponent<InteractableTag>();
        if (tag != null) tag.canBePossessed = false;
        Debug.Log($"[L3] Locked: {toy.gameObject.name}");
    }

    void SetLights(bool on)
    {
        foreach (var light in sceneLights)
            if (light != null) light.intensity = on ? litIntensity : 0f;
    }

    // ─── Phase Callbacks ────────────────────────────────────────

    public void OnCansCleared()
    {
        if (currentPhase != Phase.Idle) return;
        currentPhase = Phase.CansCleared;
        Debug.Log("[L3] Phase: CansCleared");
        Lock(springGlove);
        Unlock(switchObj);
    }

    public void OnLightsOn()
    {
        if (currentPhase != Phase.CansCleared) return;
        currentPhase = Phase.LightsOn;
        Debug.Log("[L3] Phase: LightsOn");
        SetLights(true);
        Lock(switchObj);
        if (littleGirl != null && girlWaypointRight != null)
            littleGirl.StartMovingTo(girlWaypointRight);
        Unlock(pendantLight);
    }

    public void OnLampSwung()
    {
        if (currentPhase != Phase.LightsOn) return;
        currentPhase = Phase.LampSwung;
        Debug.Log("[L3] Phase: LampSwung");
        if (littleGirl != null && girlWaypointLeft != null)
            littleGirl.StartMovingTo(girlWaypointLeft);
        if (cat != null && catWaypointLeft != null)
            cat.MoveToTarget(catWaypointLeft);
        Lock(pendantLight);
        Unlock(tableClock);
    }

    public void OnClockShaken()
    {
        if (currentPhase != Phase.LampSwung) return;
        Debug.Log("[L3] Clock shaken → Table shaking");
        Lock(tableClock);
        computerTable?.TriggerShake();
    }

    public void OnTableShakeDone()
    {
        Debug.Log("[L3] Table shake done → Plank falling");
        blank?.TriggerFall();
    }

    public void OnPlankFell()
    {
        if (currentPhase != Phase.LampSwung) return;
        currentPhase = Phase.PlankFell;
        Debug.Log("[L3] Phase: PlankFell → Girl and cat walk to plank, Apple unlocked");

        // 小女孩和小猫走到木板上等待
        if (littleGirl != null && girlWaypointOnPlank != null)
            littleGirl.StartMovingTo(girlWaypointOnPlank);
        if (cat != null && catWaypointOnPlank != null)
            cat.MoveToTarget(catWaypointOnPlank);

        Unlock(apple);
    }

    public void OnAppleCoveredCandle()
    {
        if (currentPhase != Phase.PlankFell) return;
        currentPhase = Phase.AppleDone;
        Lock(apple);
        StartCoroutine(DelayedNPCMove());
    }

    System.Collections.IEnumerator DelayedNPCMove()
    {
        yield return new WaitForSeconds(1.5f); // 等1.5秒再移动
        Debug.Log("[L3] Phase: AppleDone → Girl and cat walk to pump");
        if (littleGirl != null && girlWaypointAtPump != null)
            littleGirl.StartMovingTo(girlWaypointAtPump);
        if (cat != null && catWaypointAtPump != null)
            cat.MoveToTarget(catWaypointAtPump);
        Unlock(airPump);
    }

    public void OnBalloonFilled()
    {
        Debug.Log("[L3] Balloon filled → Inflating");
        Lock(airPump);
        balloonL3?.TriggerInflate();
    }

    public void OnDeskLampOn()
    {
        if (currentPhase != Phase.AppleDone) return;
        currentPhase = Phase.DeskLampOn;
        Debug.Log("[L3] Phase: DeskLampOn → MusicBox unlocked");
        Unlock(musicBox);
    }

    public void OnMusicPlayed()
    {
        if (currentPhase != Phase.DeskLampOn) return;
        currentPhase = Phase.MusicPlayed;
        Debug.Log("[L3] Phase: MusicPlayed → Cat jumps to desk");
        Lock(musicBox);

        // 小猫跳上桌面
        if (cat != null && catWaypointOnDesk != null)
            cat.JumpToDesk(catWaypointOnDesk);
        
    }

    public void OnCatOnDesk()
    {
        Debug.Log("[L3] Cat on desk → triggering gift box");
        giftBox?.TriggerKnock();
    }


    public void OnGiftRevealed()
    {
        if (currentPhase != Phase.MusicPlayed) return;
        currentPhase = Phase.Complete;
        Debug.Log("[L3] Phase: Complete → Girl walks over");

        if (littleGirl != null && girlWaypointFinal != null)
            littleGirl.StartMovingTo(girlWaypointFinal);

        StartCoroutine(LoadEndingDelayed());
    }

    System.Collections.IEnumerator LoadEndingDelayed()
    {
        yield return new WaitForSeconds(4f); // 等小女孩走过来再切换
        if (!string.IsNullOrEmpty(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}