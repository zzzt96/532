using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class AppleMoveWwiseTrigger : MonoBehaviour
{
    [Header("Reference")]
    public Transform appleTransform; // 拖Apple（一般留空）

    [Header("Wwise Event")]
    public WwiseEvent moveEvent; // 可选event

    [Header("Settings")]
    public float moveThreshold = 0.001f;

    private Vector3 lastPosition;
    private bool hasTriggered = false;

    void Start()
    {
        if (appleTransform == null)
            appleTransform = transform;

        lastPosition = appleTransform.position;
    }

    void Update()
    {
        if (hasTriggered || appleTransform == null) return;

        float distance = Vector3.Distance(appleTransform.position, lastPosition);

        // 🎯 一旦开始移动
        if (distance > moveThreshold)
        {
            TriggerEvent();
        }

        lastPosition = appleTransform.position;
    }

    void TriggerEvent()
    {
        hasTriggered = true;

        if (moveEvent != null && moveEvent.IsValid())
        {
            moveEvent.Post(gameObject);
            Debug.Log("[Apple] Move Event Triggered");
        }
    }
}