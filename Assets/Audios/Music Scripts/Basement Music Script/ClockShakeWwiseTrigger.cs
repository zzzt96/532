using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class ClockShakeWwiseTrigger : MonoBehaviour
{
    [Header("Reference")]
    public Transform clockTransform; // 拖你的clock（一般留空）

    [Header("Wwise Event")]
    public WwiseEvent triggerEvent;

    [Header("Settings")]
    public float moveThreshold = 0.001f; // 抖动判定

    private Vector3 lastPosition;
    private bool hasTriggered = false;

    void Start()
    {
        if (clockTransform == null)
            clockTransform = transform;

        lastPosition = clockTransform.position;
    }

    void Update()
    {
        if (hasTriggered || clockTransform == null) return;

        float distance = Vector3.Distance(clockTransform.position, lastPosition);

        // 🎯 一旦开始shake（位置发生变化）
        if (distance > moveThreshold)
        {
            TriggerEvent();
        }

        lastPosition = clockTransform.position;
    }

    void TriggerEvent()
    {
        hasTriggered = true;

        if (triggerEvent != null && triggerEvent.IsValid())
        {
            triggerEvent.Post(gameObject);
            Debug.Log("[Clock] Shake Event Triggered");
        }
    }
}