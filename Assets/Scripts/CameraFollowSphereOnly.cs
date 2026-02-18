using UnityEngine;

public class CameraFollowSphereOnly : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 1, -8);
    public float smoothSpeed = 8f;

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

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}