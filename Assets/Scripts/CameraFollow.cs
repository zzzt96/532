using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform girlTarget;   // 小女孩
    public Transform playerTarget; // 玩家灵魂（鼠标控制）

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 2, -10); // 摄像机偏移
    public float smoothSpeed = 5f;
    public float movementThreshold = 0.01f; // 判断小女孩是否在移动

    [Header("Camera Bounds")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minY = 0f;
    public float maxY = 10f;

    private Vector3 lastGirlPosition;
    private bool isGirlMoving = false;

    void Start()
    {
        if (girlTarget != null)
            lastGirlPosition = girlTarget.position;
    }

    void LateUpdate()
    {
        if (girlTarget == null || playerTarget == null) return;

        // 检测小女孩是否在移动
        float girlMovementSpeed = Vector3.Distance(girlTarget.position, lastGirlPosition) / Time.deltaTime;
        isGirlMoving = girlMovementSpeed > movementThreshold;
        lastGirlPosition = girlTarget.position;

        // 选择跟随目标
        Transform currentTarget = isGirlMoving ? girlTarget : playerTarget;
        // Debug.Log($"[Camera] Speed: {girlMovementSpeed:F3}, isMoving: {isGirlMoving}, Following: {currentTarget.name}");

        // 计算目标位置
        Vector3 targetPos = currentTarget.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}