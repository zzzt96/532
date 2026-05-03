using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class BookFallTrigger : MonoBehaviour
{
    [Header("Wwise Event")]
    public WwiseEvent fallEvent;

    [Header("Settings")]
    [Tooltip("倾斜多少角度算倒下")]
    public float fallAngleThreshold = 45f;

    private bool hasFallen = false;

    void Update()
    {
        if (hasFallen) return;

        // 计算当前物体“上方向”和世界上方向的夹角
        float angle = Vector3.Angle(transform.up, Vector3.up);

        // 超过阈值 → 判定为倒下
        if (angle > fallAngleThreshold)
        {
            TriggerFall();
        }
    }

    void TriggerFall()
    {
        hasFallen = true;

        if (fallEvent != null && fallEvent.IsValid())
        {
            fallEvent.Post(gameObject);
            Debug.Log("[Book] Fall Event Triggered");
        }
    }
}