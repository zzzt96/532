using UnityEngine;
using System.Collections;

/// <summary>
/// 风扇 - 由气球开关触发启动，玩家随后附身
/// AD 键控制风扇头左右摇摆，摇到触发角度时吹倒目标物体
/// </summary>
public class Fan : ToyBase
{
    [Header("Fan Head")]
    [Tooltip("风扇头部的子Transform，摇摆时旋转它（Y轴）")]
    public Transform fanHead;
    public float headRotateSpeed = 50f;
    [Tooltip("摇头最大角度（左右各这么多度）")]
    public float maxHeadAngle = 50f;

    [Header("Blow Target")]
    [Tooltip("被吹倒的物体，需要有 Rigidbody")]
    public Rigidbody blowTarget;
    [Tooltip("吹力方向（世界空间，通常 Vector3.left）")]
    public Vector3 blowDirection = Vector3.left;
    public float blowForce = 6f;
    [Tooltip("摇头到达这个角度后触发吹倒（通常设为 maxHeadAngle 的 60-80%）")]
    public float triggerHeadAngle = 35f;

    private bool isOn = false;
    private float currentHeadAngle = 0f;
    private bool blowTriggered = false;

    // 风扇默认不可附身，开关触发后再解锁
    void Start()
    {
        canBePossessed = false;
    }

    public override void ToyUpdate()
    {
        if (!isOn || blowTriggered) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentHeadAngle += input * headRotateSpeed * Time.deltaTime;
        currentHeadAngle = Mathf.Clamp(currentHeadAngle, -maxHeadAngle, maxHeadAngle);

        if (fanHead != null)
            fanHead.localRotation = Quaternion.Euler(0f, currentHeadAngle, 0f);

        if (Mathf.Abs(currentHeadAngle) >= triggerHeadAngle)
        {
            blowTriggered = true;
            TriggerBlow();
        }
    }

    /// <summary>由 Level2Manager 在气球触发后调用</summary>
    public void TurnOn()
    {
        isOn = true;
        // TODO: 播放风扇旋转音效/视觉
        Debug.Log("[Fan] Turned on!");
    }

    void TriggerBlow()
    {
        if (blowTarget != null)
            blowTarget.AddForce(blowDirection.normalized * blowForce, ForceMode.Impulse);

        canBePossessed = false;
        GetComponent<InteractableTag>()?.SetCompleted();
        Debug.Log("[Fan] Blow triggered!");
        Level2Manager.Instance?.OnFanBlowTriggeredChair();
    }
}