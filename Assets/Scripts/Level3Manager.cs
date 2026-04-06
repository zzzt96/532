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

    [Header("NPC References")]
    public LittleGirlController littleGirl;
    public CatNPC cat;

    [Header("Waypoints")]
    public Transform girlWaypointRight;
    public Transform girlWaypointLeft;
    public Transform catWaypointLeft;
    public Transform girlWaypointAfterPlank; // 木板倒下后女孩走过水坑

    [Header("Light References")]
    public Light[] sceneLights;
    public float litIntensity = 100f;

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
        Debug.Log("[L3] Phase: PlankFell → Girl walks over puddle, Apple unlocked");
        if (littleGirl != null && girlWaypointAfterPlank != null)
            littleGirl.StartMovingTo(girlWaypointAfterPlank);
        Unlock(apple);
    }

    public void OnAppleCoveredCandle()
    {
        if (currentPhase != Phase.PlankFell) return;
        currentPhase = Phase.AppleDone;
        Debug.Log("[L3] Phase: AppleDone → Girl continues forward");
        Lock(apple);
        // TODO: 触发小女孩继续前进到下一个waypoint
    }
}