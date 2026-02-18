using UnityEngine;

/// <summary>
/// 挂在 Player
/// 自动创建一个 Overlay Camera 只渲染幽灵的 Layer
/// 这样幽灵永远不会被其他物体遮挡
/// </summary>
public class GhostAlwaysVisible : MonoBehaviour
{
    void Start()
    {
        int ghostLayer = LayerMask.NameToLayer("Ghost");
        if (ghostLayer < 0)
        {
            ghostLayer = 6;
        }
        
        SetLayerRecursive(gameObject, ghostLayer);

        // 让主相机不渲染幽灵 Layer
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.cullingMask &= ~(1 << ghostLayer);
        }

        // 创建 Overlay Camera 只渲染幽灵
        GameObject overlayCamObj = new GameObject("GhostOverlayCamera");
        overlayCamObj.transform.SetParent(mainCam.transform);
        overlayCamObj.transform.localPosition = Vector3.zero;
        overlayCamObj.transform.localRotation = Quaternion.identity;

        Camera overlayCam = overlayCamObj.AddComponent<Camera>();
        overlayCam.clearFlags = CameraClearFlags.Depth; // 只渲染深度，不清除背景
        overlayCam.cullingMask = 1 << ghostLayer;       // 只渲染幽灵 Layer
        overlayCam.depth = mainCam.depth + 1;           // 在主相机之上渲染
        overlayCam.fieldOfView = mainCam.fieldOfView;
        overlayCam.nearClipPlane = mainCam.nearClipPlane;
        overlayCam.farClipPlane = mainCam.farClipPlane;
        
        var urpData = overlayCam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (urpData == null)
            urpData = overlayCamObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        urpData.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Overlay;

        // 把 overlay camera 添加到主相机的 camera stack
        var mainUrpData = mainCam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (mainUrpData != null)
        {
            mainUrpData.cameraStack.Add(overlayCam);
        }

        // Debug.Log($"[GhostAlwaysVisible] Overlay camera created. Ghost on layer {ghostLayer}");
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}