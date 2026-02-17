// // using UnityEngine;
    // // using System.Collections.Generic;
    // // using System.Linq;
    // //
    // // /// <summary>
    // // /// 玩家灵魂控制器 - 支持Tab键切换焦点
    // // /// 鼠标悬停时自动锁定最近的物体
    // // /// 按Tab键可以在重叠的物体之间循环切换
    // // /// </summary>
    // // public class PlayerController : MonoBehaviour
    // // {
    // //     [Header("Movement")]
    // //     public float mouseFollowSmoothing = 20f;
    // //
    // //     [Header("Movement Bounds")]
    // //     public float minX = -18.7f;
    // //     public float maxX = 18.7f;
    // //     public float minY = 0f;
    // //     public float maxY = 5f;
    // //     public float fixedZ = 0f;
    // //
    // //     [Header("Interaction Detection")]
    // //     public LayerMask interactableLayer;
    // //     public float raycastDistance = 100f;
    // //
    // //     [Header("Tab Switching")]
    // //     public KeyCode switchTargetKey = KeyCode.Tab; // Tab键切换焦点
    // //
    // //     [Header("Possession")]
    // //     public ToyBase currentHover;
    // //     public ToyBase currentToy;
    // //     public bool isPossessing = false;
    // //     public KeyCode possessKey = KeyCode.LeftShift;
    // //
    // //     [Header("Visual")]
    // //     public Color normalColor = new Color(0f, 1f, 0f, 0.6f);
    // //     public Color activeColor = new Color(1f, 0.8f, 0.8f, 1f);
    // //
    // //     private Camera mainCam;
    // //     private Renderer rend;
    // //     private List<ToyBase> availableToys = new List<ToyBase>(); // 鼠标下所有可交互物体
    // //     private int currentToyIndex = 0; // 当前选中的物体索引
    // //
    // //     void Start()
    // //     {
    // //         mainCam = Camera.main;
    // //         rend = GetComponent<Renderer>();
    // //         if (rend) rend.material.color = normalColor;
    // //         
    // //         transform.position = new Vector3(0, 1, fixedZ);
    // //         Debug.Log($"[Player] Start position: {transform.position}");
    // //     }
    // //
    // //     void Update()
    // //     {
    // //         if (GameManager.Instance != null)
    // //         {
    // //             if (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying)
    // //                 return;
    // //         }
    // //         
    // //         if (Input.GetKeyDown(possessKey))
    // //         {
    // //             if (isPossessing) ExitPossess();
    // //             else if (currentHover != null && currentHover.canBePossessed) EnterPossess();
    // //         }
    // //
    // //         // Tab键切换焦点
    // //         if (Input.GetKeyDown(switchTargetKey) && availableToys.Count > 1)
    // //         {
    // //             SwitchToNextToy();
    // //         }
    // //
    // //         if (isPossessing && currentToy != null)
    // //         {
    // //             transform.position = currentToy.transform.position;
    // //             currentToy.ToyUpdate();
    // //             return;
    // //         }
    // //         else
    // //         {
    // //             HandleMouseMovement();
    // //             DetectHoverObject();
    // //         }
    // //     }
    // //
    // //     void HandleMouseMovement()
    // //     {
    // //         Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
    // //         Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, fixedZ));
    // //         float distance;
    // //         
    // //         if (plane.Raycast(ray, out distance))
    // //         {
    // //             Vector3 mouseWorldPos = ray.GetPoint(distance);
    // //             
    // //             float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
    // //             float targetY = Mathf.Clamp(mouseWorldPos.y, minY, maxY);
    // //             Vector3 targetPos = new Vector3(targetX, targetY, fixedZ);
    // //
    // //             transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mouseFollowSmoothing);
    // //         }
    // //         
    // //         Vector3 pos = transform.position;
    // //         pos.z = fixedZ;
    // //         transform.position = pos;
    // //     }
    // //
    // //     void DetectHoverObject()
    // //     {
    // //         Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
    // //         RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, interactableLayer);
    // //
    // //         if (hits.Length == 0)
    // //         {
    // //             ClearHover();
    // //             return;
    // //         }
    // //
    // //         // 收集所有可交互物体，按距离排序
    // //         availableToys.Clear();
    // //         var sortedHits = hits
    // //             .Select(hit => new { hit.distance, toy = hit.collider.GetComponent<ToyBase>() })
    // //             .Where(x => x.toy != null && x.toy.canBePossessed)
    // //             .OrderBy(x => x.distance)
    // //             .Select(x => x.toy)
    // //             .ToList();
    // //
    // //         availableToys = sortedHits;
    // //
    // //         if (availableToys.Count == 0)
    // //         {
    // //             ClearHover();
    // //             return;
    // //         }
    // //
    // //         // 默认选择最近的物体
    // //         if (currentHover == null || !availableToys.Contains(currentHover))
    // //         {
    // //             currentToyIndex = 0;
    // //             SetHover(availableToys[0]);
    // //         }
    // //     }
    // //
    // //     /// <summary>
    // //     /// Tab键切换到下一个物体
    // //     /// </summary>
    // //     void SwitchToNextToy()
    // //     {
    // //         if (availableToys.Count == 0) return;
    // //
    // //         currentToyIndex = (currentToyIndex + 1) % availableToys.Count;
    // //         SetHover(availableToys[currentToyIndex]);
    // //
    // //         Debug.Log($"Switched to: {currentHover.name} ({currentToyIndex + 1}/{availableToys.Count})");
    // //     }
    // //
    // //     void SetHover(ToyBase toy)
    // //     {
    // //         if (currentHover != null) currentHover.OnHoverExit();
    // //         currentHover = toy;
    // //
    // //         if (currentHover != null)
    // //         {
    // //             currentHover.OnHoverEnter();
    // //             if (rend) rend.material.color = activeColor;
    // //         }
    // //     }
    // //
    // //     void ClearHover()
    // //     {
    // //         if (currentHover != null)
    // //         {
    // //             currentHover.OnHoverExit();
    // //             currentHover = null;
    // //         }
    // //         availableToys.Clear();
    // //         currentToyIndex = 0;
    // //         if (rend) rend.material.color = normalColor;
    // //     }
    // //
    // //     void EnterPossess()
    // //     {
    // //         isPossessing = true;
    // //         currentToy = currentHover;
    // //         currentToy.Possess();
    // //         if (rend) rend.enabled = false;
    // //     }
    // //
    // //     public void ExitPossess()
    // //     {
    // //         if (currentToy != null) currentToy.UnPossess();
    // //         isPossessing = false;
    // //         currentToy = null;
    // //         if (rend)
    // //         {
    // //             rend.enabled = true;
    // //             rend.material.color = normalColor;
    // //         }
    // //     }
    // // }
    //
    //
    // using UnityEngine;
    // using System.Collections.Generic;
    // using System.Linq;
    // public class PlayerController : MonoBehaviour
    // {
    //     [Header("Movement")]
    //     public float mouseFollowSmoothing = 5f;
    //
    //     [Header("Movement Bounds")]
    //     public float minX = -40f;
    //     public float maxX = 10f;
    //     public float minY = 2f;
    //     public float maxY = 12f;
    //     public float defaultZ = -6f;  // 默认 Z 轴位置
    //     
    //     [Header("Dynamic Z Settings")]
    //     public bool allowDynamicZ = true;   // 是否允许动态调整 Z
    //     public float zSmoothSpeed = 10f;    // Z 轴平滑移动速度
    //     public float zDetectionRange = 5f;  // Z 轴检测范围（前后各多少距离）
    //
    //     [Header("Interaction Detection")]
    //     public float raycastDistance = 100f;
    //
    //     [Header("Tab Switching")]
    //     public KeyCode switchTargetKey = KeyCode.Tab;
    //
    //     [Header("Possession")]
    //     public ToyBase currentHover;
    //     public ToyBase currentToy;
    //     public bool isPossessing = false;
    //     public KeyCode possessKey = KeyCode.LeftShift;
    //
    //     [Header("Visual")]
    //     public Color normalColor = new Color(0.35f, 0.58f, 0.55f, 0.6f);
    //     public Color activeColor = new Color(1f, 0.8f, 0.8f, 1f);
    //
    //     private Camera mainCam;
    //     private Renderer rend;
    //     private List<ToyBase> availableToys = new List<ToyBase>();
    //     private int currentToyIndex = 0;
    //     private float targetZ;  
    //     
    //     void Start()
    //     {
    //         mainCam = Camera.main;
    //         rend = GetComponent<Renderer>();
    //         if (rend) rend.material.color = normalColor;
    //         
    //         transform.position = new Vector3(0, 1, defaultZ);
    //         targetZ = defaultZ;
    //         
    //         // Debug.Log($"[Player] Start position: {transform.position}");
    //         // Debug.Log($"[Player] Dynamic Z enabled: {allowDynamicZ}");
    //     }
    //
    //     void Update()
    //     {
    //         if (GameManager.Instance != null)
    //         {
    //             if (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying)
    //                 return;
    //         }
    //         
    //         if (Input.GetKeyDown(possessKey))
    //         {
    //             if (isPossessing) ExitPossess();
    //             else if (currentHover != null && currentHover.canBePossessed) EnterPossess();
    //         }
    //
    //         if (Input.GetKeyDown(switchTargetKey) && availableToys.Count > 1)
    //         {
    //             SwitchToNextToy();
    //         }
    //
    //         if (isPossessing && currentToy != null)
    //         {
    //             transform.position = currentToy.transform.position;
    //             currentToy.ToyUpdate();
    //             return;
    //         }
    //         else
    //         {
    //             HandleMouseMovement();
    //             DetectHoverObject();
    //             
    //             // 动态调整 Z 轴
    //             if (allowDynamicZ)
    //             {
    //                 UpdateDynamicZ();
    //             }
    //             else
    //             {
    //                 // 如果不允许动态 Z，强制锁定在默认 Z
    //                 Vector3 pos = transform.position;
    //                 pos.z = defaultZ;
    //                 transform.position = pos;
    //             }
    //         }
    //     }
    //
    //     void HandleMouseMovement()
    //     {
    //         Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
    //         // 使用当前的 Z（而不是固定的 defaultZ）
    //         Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
    //         float distance;
    //         
    //         if (plane.Raycast(ray, out distance))
    //         {
    //             Vector3 mouseWorldPos = ray.GetPoint(distance);
    //             
    //             float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
    //             float targetY = Mathf.Clamp(mouseWorldPos.y, minY, maxY);
    //             Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
    //
    //             Vector3 newPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mouseFollowSmoothing);
    //             // 保持当前 Z（由 UpdateDynamicZ 控制）
    //             newPos.z = transform.position.z;
    //             transform.position = newPos;
    //         }
    //     }
    //
    //     void UpdateDynamicZ()
    //     {
    //         // 根据悬停的物体调整目标 Z
    //         if (currentHover != null)
    //         {
    //             float newTargetZ = currentHover.transform.position.z;
    //             if (Mathf.Abs(newTargetZ - targetZ) > 0.1f)  // Z 有明显变化时输出日志
    //             {
    //                 Debug.Log($"[Player] Z changing: {targetZ:F2} → {newTargetZ:F2} (Hover: {currentHover.name})");
    //             }
    //             targetZ = newTargetZ;
    //         }
    //         else
    //         {
    //             if (Mathf.Abs(defaultZ - targetZ) > 0.1f)  // 回到默认 Z 时输出日志
    //             {
    //                 Debug.Log($"[Player] Z returning to default: {targetZ:F2} → {defaultZ:F2}");
    //             }
    //             targetZ = defaultZ;  // 没有悬停物体时回到默认 Z
    //         }
    //         
    //         // 平滑移动到目标 Z
    //         Vector3 pos = transform.position;
    //         pos.z = Mathf.Lerp(pos.z, targetZ, Time.deltaTime * zSmoothSpeed);
    //         transform.position = pos;
    //     }
    //
    //     void DetectHoverObject()
    //     {
    //         Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
    //         
    //         // 检测所有物体（不使用 LayerMask）
    //         RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance);
    //
    //         if (hits.Length == 0)
    //         {
    //             ClearHover();
    //             return;
    //         }
    //
    //         // 收集所有可交互物体（通过检测 ToyBase 组件）
    //         availableToys.Clear();
    //         var sortedHits = hits
    //             .Select(hit => new { hit.distance, toy = hit.collider.GetComponent<ToyBase>() })
    //             .Where(x => x.toy != null && x.toy.canBePossessed)
    //             .OrderBy(x => x.distance)  // 按射线距离排序，最近的优先
    //             .Select(x => x.toy)
    //             .ToList();
    //
    //         availableToys = sortedHits;
    //
    //         if (availableToys.Count == 0)
    //         {
    //             ClearHover();
    //             return;
    //         }
    //
    //         // 默认选择最近的物体
    //         if (currentHover == null || !availableToys.Contains(currentHover))
    //         {
    //             currentToyIndex = 0;
    //             SetHover(availableToys[0]);
    //             
    //             // Debug：输出找到的可交互物体
    //             Debug.Log($"[Player] Found {availableToys.Count} interactable objects. Hovering: {currentHover.name} at Z={currentHover.transform.position.z:F2}");
    //         }
    //     }
    //
    //     void SwitchToNextToy()
    //     {
    //         if (availableToys.Count == 0) return;
    //
    //         currentToyIndex = (currentToyIndex + 1) % availableToys.Count;
    //         SetHover(availableToys[currentToyIndex]);
    //
    //         Debug.Log($"Switched to: {currentHover.name} ({currentToyIndex + 1}/{availableToys.Count})");
    //     }
    //
    //     void SetHover(ToyBase toy)
    //     {
    //         if (currentHover != null) currentHover.OnHoverExit();
    //         currentHover = toy;
    //
    //         if (currentHover != null)
    //         {
    //             currentHover.OnHoverEnter();
    //             if (rend) rend.material.color = activeColor;
    //         }
    //     }
    //
    //     void ClearHover()
    //     {
    //         if (currentHover != null)
    //         {
    //             currentHover.OnHoverExit();
    //             currentHover = null;
    //         }
    //         availableToys.Clear();
    //         currentToyIndex = 0;
    //         if (rend) rend.material.color = normalColor;
    //     }
    //
    //     void EnterPossess()
    //     {
    //         isPossessing = true;
    //         currentToy = currentHover;
    //         currentToy.Possess();
    //         if (rend) rend.enabled = false;
    //     }
    //
    //     public void ExitPossess()
    //     {
    //         if (currentToy != null) currentToy.UnPossess();
    //         isPossessing = false;
    //         currentToy = null;
    //         if (rend)
    //         {
    //             rend.enabled = true;
    //             rend.material.color = normalColor;
    //         }
    //     }
    // }

    
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float mouseFollowSmoothing = 3f;   

    [Header("Movement Bounds")]
    public float minX = -40f;
    public float maxX = 10f;
    public float minY = 2f;
    public float maxY = 12f;
    public float defaultZ = -6f;

    [Header("Dynamic Z Settings")]
    public bool allowDynamicZ = true;
    public float zSmoothSpeed = 10f;
    public float zDetectionRange = 5f;

    [Header("Interaction Detection")]
    public float raycastDistance = 100f;

    [Header("Tab Switching")]
    public KeyCode switchTargetKey = KeyCode.Tab;

    [Header("Possession")]
    public ToyBase currentHover;
    public ToyBase currentToy;
    public bool isPossessing = false;
    public KeyCode possessKey = KeyCode.LeftShift;

    [Header("Visual")]
    public Color normalColor = new Color(0.35f, 0.58f, 0.55f, 0.6f);
    public Color activeColor = new Color(1f, 0.8f, 0.8f, 1f);

    [Header("Camera Zoom")]
    public float normalFOV = 60f;           
    public float possessFOV = 35f;          
    public float fovSmoothSpeed = 5f;     

    private Camera mainCam;
    private Renderer rend;
    private List<ToyBase> availableToys = new List<ToyBase>();
    private int currentToyIndex = 0;
    private float targetZ;
    private float targetFOV;          

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = normalColor;

        transform.position = new Vector3(0, 1, defaultZ);
        targetZ = defaultZ;
        
        targetFOV = normalFOV;
        if (mainCam) mainCam.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying)
                return;
        }

        if (Input.GetKeyDown(possessKey))
        {
            if (isPossessing) ExitPossess();
            else if (currentHover != null && currentHover.canBePossessed) EnterPossess();
        }

        if (Input.GetKeyDown(switchTargetKey) && availableToys.Count > 1)
        {
            SwitchToNextToy();
        }
        
        if (mainCam && Mathf.Abs(mainCam.fieldOfView - targetFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
        }

        if (isPossessing && currentToy != null)
        {
            transform.position = currentToy.transform.position;
            currentToy.ToyUpdate();
            return;
        }
        else
        {
            HandleMouseMovement();
            DetectHoverObject();

            if (allowDynamicZ)
            {
                UpdateDynamicZ();
            }
            else
            {
                Vector3 pos = transform.position;
                pos.z = defaultZ;
                transform.position = pos;
            }
        }
    }

    void HandleMouseMovement()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
            float targetY = Mathf.Clamp(mouseWorldPos.y, minY, maxY);
            Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
            
            Vector3 newPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mouseFollowSmoothing);
            newPos.z = transform.position.z;
            transform.position = newPos;
        }
    }

    void UpdateDynamicZ()
    {
        if (currentHover != null)
        {
            float newTargetZ = currentHover.transform.position.z;
            if (Mathf.Abs(newTargetZ - targetZ) > 0.1f)
            {
                Debug.Log($"[Player] Z changing: {targetZ:F2} → {newTargetZ:F2} (Hover: {currentHover.name})");
            }
            targetZ = newTargetZ;
        }
        else
        {
            if (Mathf.Abs(defaultZ - targetZ) > 0.1f)
            {
                Debug.Log($"[Player] Z returning to default: {targetZ:F2} → {defaultZ:F2}");
            }
            targetZ = defaultZ;
        }

        Vector3 pos = transform.position;
        pos.z = Mathf.Lerp(pos.z, targetZ, Time.deltaTime * zSmoothSpeed);
        transform.position = pos;
    }

    void DetectHoverObject()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance);

        if (hits.Length == 0)
        {
            ClearHover();
            return;
        }

        availableToys.Clear();
        var sortedHits = hits
            .Select(hit => new { hit.distance, toy = hit.collider.GetComponent<ToyBase>() })
            .Where(x => x.toy != null && x.toy.canBePossessed)
            .OrderBy(x => x.distance)
            .Select(x => x.toy)
            .ToList();

        availableToys = sortedHits;

        if (availableToys.Count == 0)
        {
            ClearHover();
            return;
        }

        if (currentHover == null || !availableToys.Contains(currentHover))
        {
            currentToyIndex = 0;
            SetHover(availableToys[0]);
            Debug.Log($"[Player] Found {availableToys.Count} interactable objects. Hovering: {currentHover.name} at Z={currentHover.transform.position.z:F2}");
        }
    }

    void SwitchToNextToy()
    {
        if (availableToys.Count == 0) return;
        currentToyIndex = (currentToyIndex + 1) % availableToys.Count;
        SetHover(availableToys[currentToyIndex]);
        Debug.Log($"Switched to: {currentHover.name} ({currentToyIndex + 1}/{availableToys.Count})");
    }

    void SetHover(ToyBase toy)
    {
        if (currentHover != null) currentHover.OnHoverExit();
        currentHover = toy;

        if (currentHover != null)
        {
            currentHover.OnHoverEnter();
            if (rend) rend.material.color = activeColor;
        }
    }

    void ClearHover()
    {
        if (currentHover != null)
        {
            currentHover.OnHoverExit();
            currentHover = null;
        }
        availableToys.Clear();
        currentToyIndex = 0;
        if (rend) rend.material.color = normalColor;
    }

    void EnterPossess()
    {
        isPossessing = true;
        currentToy = currentHover;
        currentToy.Possess();
        if (rend) rend.enabled = false;

        // Zoom in
        targetFOV = possessFOV;
        // Debug.Log($"[Player] Possessed {currentToy.name}, zooming in to FOV={possessFOV}");
    }

    public void ExitPossess()
    {
        if (currentToy != null) currentToy.UnPossess();
        isPossessing = false;
        currentToy = null;
        if (rend)
        {
            rend.enabled = true;
            rend.material.color = normalColor;
        }

        //  Zoom out
        targetFOV = normalFOV;
        // Debug.Log("[Player] Exited possession, zooming out to FOV=" + normalFOV);
    }
}