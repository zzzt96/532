using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class AtticLayerTwoMusicTrigger : MonoBehaviour
{
    [Header("Wwise Event")]
    public WwiseEvent moveEvent;

    [Header("Settings")]
    public float moveThreshold = 0.01f;

    private Vector3 lastPosition;
    private bool hasTriggered = false;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 已经触发过就不再检测
        if (hasTriggered) return;

        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > moveThreshold)
        {
            if (moveEvent != null && moveEvent.IsValid())
            {
                moveEvent.Post(gameObject);
                Debug.Log("[MoveTrigger] Event Triggered");
            }

            hasTriggered = true; // ✅ 只触发一次
        }

        lastPosition = transform.position;
    }
}