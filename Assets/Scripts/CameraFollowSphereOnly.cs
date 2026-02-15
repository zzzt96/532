using UnityEngine;

/// <summary>
/// 摄像机跟随 - 只跟随玩家 Sphere
/// FOV 窄，只能看到部分房间
/// </summary>
public class CameraFollowSphereOnly : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;  // 拖入 Sphere
    
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 1, -8);
    public float smoothSpeed = 8f;
    public float fieldOfView = 20f;  // 窄视野
    
    [Header("Camera Bounds")]
    public float minX = -15f;  // 左墙
    public float maxX = 15f;   // 右墙
    public float minY = 0f;
    public float maxY = 10f;
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }
    }
    
    void LateUpdate()
    {
        if (playerTarget == null) return;
        
        // 计算目标位置
        Vector3 targetPos = playerTarget.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        
        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}