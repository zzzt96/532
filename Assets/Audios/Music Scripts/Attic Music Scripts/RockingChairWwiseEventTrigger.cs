using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class RockingChairWwiseEventTrigger : MonoBehaviour
{
    [Header("Reference")]
    public Transform chairTransform; // 拖你的椅子（一般就是自己）

    [Header("Wwise Event")]
    public WwiseEvent triggerEvent;

    [Header("Settings")]
    public float rotationThreshold = 1f; // 多少角度算开始摇

    private Quaternion lastRotation;
    private bool hasTriggered = false;

    void Start()
    {
        if (chairTransform == null)
            chairTransform = transform;

        lastRotation = chairTransform.rotation;
    }

    void Update()
    {
        if (hasTriggered || chairTransform == null) return;

        float angle = Quaternion.Angle(chairTransform.rotation, lastRotation);

        // 🎯 开始摇动（旋转发生变化）
        if (angle > rotationThreshold)
        {
            TriggerEvent();
        }

        lastRotation = chairTransform.rotation;
    }

    void TriggerEvent()
    {
        hasTriggered = true;

        if (triggerEvent != null && triggerEvent.IsValid())
        {
            triggerEvent.Post(gameObject);
            Debug.Log("[RockingChair] Wwise Event Triggered");
        }
    }
}