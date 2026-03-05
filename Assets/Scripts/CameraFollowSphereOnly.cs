using UnityEngine;

public class CameraFollowSphereOnly : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 1, -8);

    [Header("Smooth Speed")]
    public float smoothSpeedX = 8f;
    public float smoothSpeedY = 3f;  // Y轴单独调低，避免上下迟钝感

    [Header("Camera Bounds")]
    public float minX = -15f;
    public float maxX = 15f;
    public float minY = -5f;
    public float maxY = 10f;

    void LateUpdate()
    {
        if (playerTarget == null) return;

        Vector3 targetPos = playerTarget.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        float newX = Mathf.Lerp(transform.position.x, targetPos.x, Time.deltaTime * smoothSpeedX);
        float newY = Mathf.Lerp(transform.position.y, targetPos.y, Time.deltaTime * smoothSpeedY);

        transform.position = new Vector3(newX, newY, targetPos.z);
    }
}