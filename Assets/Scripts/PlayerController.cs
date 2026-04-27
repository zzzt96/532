// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
//
// public class PlayerController : MonoBehaviour
// {
//     [Header("Spawn Settings")]
//     public Vector2 startPosition = new Vector2(0, 1);
//     
//     // [Header("Movement")]
//     // public float mouseFollowSmoothing = 3f;
//     // public float maxTargetJumpPerFrame = 1.5f;
//     // public float mouseSensitivity = 0.15f; 
//     
//     [Header("Movement")]
//     public float mouseResponseTime = 0.04f;  // 接近0=立即，0.1=有延迟感
//     public float maxGhostSpeed = 25f;        // 降低这个来减速，建议15~40
//     private Vector3 ghostVelocity = Vector3.zero;
//     private Vector2 lastMousePos;
//     private float mouseIdleTimer = 0f;
//     public float mouseIdleThreshold = 0.05f; // 鼠标停止多少秒后才认为静止
//     
//     [Header("Movement Bounds")]
//     public float minX = -40f;
//     public float maxX = 10f;
//     public float minY = 2f;
//     public float maxY = 12f;
//     public float defaultZ = -6f;
//
//     [Header("Dynamic Z Settings")]
//     public bool allowDynamicZ = true;
//     public float zSmoothSpeed = 10f;
//     public float zDetectionRange = 5f;
//
//     [Header("Ghost-Based Detection")]
//     public float possessRadius = 1.5f;
//
//     [Header("Tab Switching")]
//     public KeyCode switchTargetKey = KeyCode.Tab;
//
//     [Header("Possession")]
//     public ToyBase currentHover;
//     public ToyBase currentToy;
//     public bool isPossessing = false;
//     public float holdDuration = 0.8f;   // 长按多久附身
//     private float holdTimer = 0f;
//
//     [Header("Visual")]
//     public Color normalColor = new Color(0.35f, 0.58f, 0.55f, 0.6f);
//     public Color activeColor = new Color(1f, 0.8f, 0.8f, 1f);
//     public Color possessedHighlightColor = Color.yellow; // 新增：附身时的描边颜色
//
//     [Header("Ghost Visibility")]
//     public float ghostZOffset = -1.5f;
//     public float normalFOV = 60f;
//     public float possessFOV = 35f;
//     public float fovSmoothSpeed = 5f;
//
//     [Header("Ghost Juice (Visual Effects)")]
//     public bool enableJuice = true;
//     public float bobAmplitude = 0.08f;
//     public float bobFrequency = 2.5f;
//     public float scaleAmplitude = 0.05f;
//     public float scaleFrequency = 2f;
//     
//     [Header("Audio")]
//     public AudioClip possessEnterSound;
//     public AudioClip possessExitSound;
//
//     // 供 CutsceneManager 调用：过场期间锁住玩家输入
//     [HideInInspector] public bool inputLocked = false;
//
//     private Camera mainCam;
//     private Renderer rend;
//     private List<ToyBase> availableToys = new List<ToyBase>();
//     private int currentToyIndex = 0;
//     private float targetZ;
//     private float targetFOV;
//     private AudioSource audioSrc;
//
//     private Vector3 originalScale;
//     private float currentBobOffset = 0f;
//
//     void Start()
//     {
//         audioSrc = GetComponent<AudioSource>();
//
//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Confined;
//
//         mainCam = Camera.main;
//         rend = GetComponent<Renderer>();
//         if (rend) rend.material.color = normalColor;
//
//         transform.position = new Vector3(startPosition.x, startPosition.y, defaultZ + ghostZOffset);
//         targetZ = defaultZ;
//
//         targetFOV = normalFOV;
//         if (mainCam) mainCam.fieldOfView = normalFOV;
//
//         originalScale = transform.localScale;
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
//         // 过场动画期间锁定玩家
//         if (inputLocked) return;
//
//         // FOV 平滑过渡
//         if (mainCam && Mathf.Abs(mainCam.fieldOfView - targetFOV) > 0.1f)
//         {
//             mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
//         }
//
//         if (isPossessing && currentToy != null)
//         {
//             transform.position = currentToy.transform.position + new Vector3(0, currentToy.cameraYOffset, 0);
//             currentToy.ToyUpdate();
//             if (Input.GetMouseButtonDown(0)) ExitPossess(); // 右键退出附身
//             return;
//         }
//         // 自由移动状态
//         HandleMouseMovement();
//
//         if (allowDynamicZ)
//             UpdateDynamicZ();
//         else
//         {
//             Vector3 pos = transform.position;
//             pos.z = defaultZ + ghostZOffset;
//             transform.position = pos;
//         }
//
//         ApplyGhostJuice();
//         DetectHoverObject();
//
//         if (Input.GetKeyDown(switchTargetKey) && availableToys.Count > 1)
//             SwitchToNextToy();
//
//         HandlePossessInput();
//     }
//     
//     void HandleMouseMovement()
//     {
//         if (isPossessing) return;
//
//         Vector2 currentMousePos = Input.mousePosition;
//     
//         if (currentMousePos != lastMousePos)
//         {
//             mouseIdleTimer = 0f; // 鼠标有移动，重置计时器
//         }
//         else
//         {
//             mouseIdleTimer += Time.deltaTime;
//         }
//         lastMousePos = currentMousePos;
//
//         // 只有真正静止超过阈值时才清零速度
//         if (mouseIdleTimer >= mouseIdleThreshold)
//         {
//             ghostVelocity = Vector3.zero;
//             return;
//         }
//
//         Ray ray = mainCam.ScreenPointToRay(currentMousePos);
//         Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, defaultZ + ghostZOffset));
//         float distance;
//
//         if (plane.Raycast(ray, out distance))
//         {
//             Vector3 mouseWorldPos = ray.GetPoint(distance);
//             float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
//             float targetY = Mathf.Clamp(mouseWorldPos.y, minY, maxY);
//
//             Vector3 logicalPos = transform.position;
//             logicalPos.y -= currentBobOffset;
//             Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
//
//             Vector3 newPos = Vector3.SmoothDamp(
//                 logicalPos, targetPos,
//                 ref ghostVelocity,
//                 mouseResponseTime,
//                 maxGhostSpeed
//             );
//
//             currentBobOffset = enableJuice ? Mathf.Sin(Time.time * bobFrequency) * bobAmplitude : 0f;
//             newPos.y += currentBobOffset;
//             newPos.z = transform.position.z;
//             transform.position = newPos;
//         }
//     }
//     
//     void ApplyGhostJuice()
//     {
//         if (!enableJuice) return;
//         float scaleOffset = Mathf.Sin(Time.time * scaleFrequency) * scaleAmplitude;
//         transform.localScale = originalScale * (1f + scaleOffset);
//     }
//
//     void UpdateDynamicZ()
//     {
//         if (currentHover != null)
//         {
//             float newTargetZ = currentHover.transform.position.z;
//             // 只允许Z往背景方向移动，不允许往摄像机方向超过defaultZ
//             newTargetZ = Mathf.Min(newTargetZ, defaultZ);
//             targetZ = newTargetZ;
//         }
//         else
//         {
//             targetZ = defaultZ;
//         }
//
//         Vector3 pos = transform.position;
//         pos.z = Mathf.Lerp(pos.z, targetZ + ghostZOffset, Time.deltaTime * zSmoothSpeed);
//         transform.position = pos;
//     }
//     
//     void DetectHoverObject()
//     {
//         ToyBase[] allToys = FindObjectsOfType<ToyBase>();
//
//         availableToys.Clear();
//         foreach (var toy in allToys)
//         {
//             if (!toy.canBePossessed) continue;
//
//             // 使用detectionOffset让高处物体把检测点下移
//             Vector3 toyDetectPos = toy.transform.position + toy.detectionOffset;
//
//             float dist2D;
//             if (toy.useXOnlyDetection)
//             {
//                 dist2D = Mathf.Abs(transform.position.x - toyDetectPos.x);
//             }
//             else
//             {
//                 Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);
//                 Vector2 toyXY   = new Vector2(toyDetectPos.x, toyDetectPos.y);
//                 dist2D = Vector2.Distance(ghostXY, toyXY);
//             }
//
//             if (dist2D <= possessRadius)
//                 availableToys.Add(toy);
//         }
//
//         // XY距离 + Z惩罚排序
//         availableToys.Sort((a, b) =>
//         {
//             Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);
//
//             Vector3 detectPosA = a.transform.position + a.detectionOffset;
//             Vector3 detectPosB = b.transform.position + b.detectionOffset;
//
//             float xyA = Vector2.Distance(ghostXY, new Vector2(detectPosA.x, detectPosA.y));
//             float xyB = Vector2.Distance(ghostXY, new Vector2(detectPosB.x, detectPosB.y));
//
//             float zPenaltyA = Mathf.Abs(transform.position.z - a.transform.position.z) * 0.5f;
//             float zPenaltyB = Mathf.Abs(transform.position.z - b.transform.position.z) * 0.5f;
//
//             return (xyA + zPenaltyA).CompareTo(xyB + zPenaltyB);
//         });
//
//         if (availableToys.Count == 0)
//         {
//             ClearHover();
//             return;
//         }
//
//         if (currentHover == null || !availableToys.Contains(currentHover))
//         {
//             currentToyIndex = 0;
//             SetHover(availableToys[0]);
//             Debug.Log($"[Player] Hovering: {currentHover.name}");
//         }
//     }
//     
//     void HandlePossessInput()
//     {
//         if (currentHover == null || !currentHover.canBePossessed)
//         {
//             holdTimer = 0f;
//             PossessUI.Instance?.Hide();
//             return;
//         }
//
//         if (Input.GetMouseButton(0)) // 长按左键
//         {
//             holdTimer += Time.deltaTime;
//             float progress = Mathf.Clamp01(holdTimer / holdDuration);
//             PossessUI.Instance?.Show(currentHover.transform.position + currentHover.uiOffset, progress);
//
//             if (holdTimer >= holdDuration)
//             {
//                 holdTimer = 0f;
//                 PossessUI.Instance?.Hide();
//                 EnterPossess();
//             }
//         }
//         else
//         {
//             // 松开重置
//             holdTimer = 0f;
//             PossessUI.Instance?.Hide();
//         }
//     }
//     void SwitchToNextToy()
//     {
//         if (availableToys.Count == 0) return;
//         currentToyIndex = (currentToyIndex + 1) % availableToys.Count;
//         SetHover(availableToys[currentToyIndex]);
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
//
//         if (audioSrc && possessEnterSound) audioSrc.PlayOneShot(possessEnterSound);
//
//         targetFOV = possessFOV;
//         Debug.Log($"[Player] Possessed {currentToy.name}");
//
//         transform.localScale = originalScale;
//         currentBobOffset = 0f;
//
//         // 【新增】：通知玩具的 Tag 强制开启高亮锁
//         InteractableTag tag = currentToy.GetComponent<InteractableTag>();
//         if (tag != null)
//         {
//             tag.SetPossessedState(true, possessedHighlightColor);
//         }
//     }
//
//     public void ExitPossess()
//     {
//         // 【新增】：通知玩具的 Tag 解除高亮锁 (要在 currentToy = null 之前调用)
//         if (currentToy != null)
//         {
//             InteractableTag tag = currentToy.GetComponent<InteractableTag>();
//             if (tag != null)
//             {
//                 tag.SetPossessedState(false, possessedHighlightColor);
//             }
//
//             currentToy.UnPossess();
//         }
//
//         isPossessing = false;
//         currentToy = null;
//         if (rend)
//         {
//             rend.enabled = true;
//             rend.material.color = normalColor;
//         }
//
//         if (audioSrc && possessExitSound) audioSrc.PlayOneShot(possessExitSound);
//
//         targetFOV = normalFOV;
//         
//         ClearHover();
//         DetectHoverObject(); // 立即重新检测，不等下一帧
//         Debug.Log("[Player] Exited possession");
//     }
// }


// keyboard version
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector2 startPosition = new Vector2(0, 1);

    [Header("Movement")]
    public float moveSpeed = 15f;

    [Header("Movement Bounds")]
    public float minX = -40f;
    public float maxX = 10f;
    public float minY = 2f;
    public float maxY = 12f;
    public float defaultZ = -6f;

    [Header("Dynamic Z Settings")]
    public bool allowDynamicZ = true;
    public float zSmoothSpeed = 2f;
    public float zDetectionRange = 5f;

    [Header("Ghost-Based Detection")]
    public float possessRadius = 1.5f;

    [Header("Tab Switching")]
    public KeyCode switchTargetKey = KeyCode.Tab;

    [Header("Possession")]
    public ToyBase currentHover;
    public ToyBase currentToy;
    public bool isPossessing = false;
    public float holdDuration = 0.8f;
    private float holdTimer = 0f;

    [Header("Visual")]
    public Color normalColor = new Color(0.35f, 0.58f, 0.55f, 0.6f);
    public Color activeColor = new Color(1f, 0.8f, 0.8f, 1f);
    public Color possessedHighlightColor = Color.yellow;

    [Header("Ghost Visibility")]
    public float ghostZOffset = -1.5f;
    public float normalFOV = 55f;
    public float possessFOV = 15f;
    public float fovSmoothSpeed = 5f;

    [Header("Ghost Juice (Visual Effects)")]
    public bool enableJuice = true;
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 2.5f;
    public float scaleAmplitude = 0.05f;
    public float scaleFrequency = 2f;

    [Header("Audio")]
    public AudioClip possessEnterSound;
    public AudioClip possessExitSound;

    [HideInInspector] public bool inputLocked = false;

    private Camera mainCam;
    private Renderer rend;
    private List<ToyBase> availableToys = new List<ToyBase>();
    private int currentToyIndex = 0;
    private float targetZ;
    private float targetFOV;
    private AudioSource audioSrc;
    private Vector3 originalScale;
    private float currentBobOffset = 0f;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = normalColor;

        transform.position = new Vector3(startPosition.x, startPosition.y, defaultZ + ghostZOffset);
        targetZ = defaultZ;
        targetFOV = normalFOV;
        if (mainCam) mainCam.fieldOfView = normalFOV;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameOver || GameManager.Instance.isIntroPlaying)
                return;
        }

        if (inputLocked) return;

        // FOV 平滑过渡
        if (mainCam && Mathf.Abs(mainCam.fieldOfView - targetFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
        }

        if (isPossessing && currentToy != null)
        {
            transform.position = currentToy.transform.position + new Vector3(0, currentToy.cameraYOffset, 0);
            currentToy.ToyUpdate();
            if (Input.GetKeyDown(KeyCode.LeftShift)) ExitPossess(); // ← 这里
            return;
        }

        // 附身前
        HandleKeyboardMovement();

        if (allowDynamicZ)
            UpdateDynamicZ();
        else
        {
            Vector3 pos = transform.position;
            pos.z = defaultZ + ghostZOffset;
            transform.position = pos;
        }

        ApplyGhostJuice();
        DetectHoverObject();

        if (Input.GetKeyDown(switchTargetKey) && availableToys.Count > 1)
            SwitchToNextToy();

        HandlePossessInput(); // 附身前空格用来附身
    }

    void HandleKeyboardMovement()
    {
        if (isPossessing) return;
        
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h += 1f; 
        if (Input.GetKey(KeyCode.D)) h -= 1f;  
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        Vector3 pos = transform.position;
        float logicalY = pos.y - currentBobOffset;

        pos.x = Mathf.Clamp(pos.x + h * moveSpeed * Time.deltaTime, minX, maxX);
        logicalY = Mathf.Clamp(logicalY + v * moveSpeed * Time.deltaTime, minY, maxY);

        currentBobOffset = enableJuice ? Mathf.Sin(Time.time * bobFrequency) * bobAmplitude : 0f;
        pos.y = logicalY + currentBobOffset;
        pos.z = transform.position.z;
        transform.position = pos;
    }
    
    void ApplyGhostJuice()
    {
        if (!enableJuice) return;
        float scaleOffset = Mathf.Sin(Time.time * scaleFrequency) * scaleAmplitude;
        transform.localScale = originalScale * (1f + scaleOffset);
    }

    void UpdateDynamicZ()
    {
        if (currentHover != null)
        {
            float newTargetZ = currentHover.transform.position.z;
            newTargetZ = Mathf.Min(newTargetZ, defaultZ);
            targetZ = newTargetZ;
        }
        else
        {
            targetZ = defaultZ;
        }

        Vector3 pos = transform.position;
        pos.z = Mathf.Lerp(pos.z, targetZ + ghostZOffset, Time.deltaTime * zSmoothSpeed);
        transform.position = pos;
    }

    void DetectHoverObject()
    {
        ToyBase[] allToys = FindObjectsOfType<ToyBase>();
        availableToys.Clear();

        foreach (var toy in allToys)
        {
            if (!toy.canBePossessed) continue;

            Vector3 toyDetectPos = toy.transform.position + toy.detectionOffset;
            float dist2D;

            if (toy.useXOnlyDetection)
            {
                dist2D = Mathf.Abs(transform.position.x - toyDetectPos.x);
            }
            else
            {
                Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);
                Vector2 toyXY = new Vector2(toyDetectPos.x, toyDetectPos.y);
                dist2D = Vector2.Distance(ghostXY, toyXY);
            }

            if (dist2D <= possessRadius)
                availableToys.Add(toy);
        }

        availableToys.Sort((a, b) =>
        {
            Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);
            Vector3 detectPosA = a.transform.position + a.detectionOffset;
            Vector3 detectPosB = b.transform.position + b.detectionOffset;
            float xyA = Vector2.Distance(ghostXY, new Vector2(detectPosA.x, detectPosA.y));
            float xyB = Vector2.Distance(ghostXY, new Vector2(detectPosB.x, detectPosB.y));
            float zPenaltyA = Mathf.Abs(transform.position.z - a.transform.position.z) * 0.5f;
            float zPenaltyB = Mathf.Abs(transform.position.z - b.transform.position.z) * 0.5f;
            return (xyA + zPenaltyA).CompareTo(xyB + zPenaltyB);
        });

        if (availableToys.Count == 0)
        {
            ClearHover();
            return;
        }

        if (currentHover == null || !availableToys.Contains(currentHover))
        {
            currentToyIndex = 0;
            SetHover(availableToys[0]);
            Debug.Log($"[Player] Hovering: {currentHover.name}");
        }
    }

    void HandlePossessInput()
    {
        if (currentHover == null || !currentHover.canBePossessed)
        {
            holdTimer = 0f;
            PossessUI.Instance?.Hide();
            return;
        }

        if (Input.GetKey(KeyCode.Space)) // 长按空格附身
        {
            holdTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(holdTimer / holdDuration);
            PossessUI.Instance?.Show(currentHover.transform.position + currentHover.uiOffset, progress);

            if (holdTimer >= holdDuration)
            {
                holdTimer = 0f;
                PossessUI.Instance?.Hide();
                EnterPossess();
            }
        }
        else
        {
            holdTimer = 0f;
            PossessUI.Instance?.Hide();
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
        if (audioSrc && possessEnterSound) audioSrc.PlayOneShot(possessEnterSound);
        targetFOV = possessFOV;
        // Debug.Log($"[Player] Possessed {currentToy.name}");
        transform.localScale = originalScale;
        currentBobOffset = 0f;

        InteractableTag tag = currentToy.GetComponent<InteractableTag>();
        if (tag != null) tag.SetPossessedState(true, possessedHighlightColor);
    }

    public void ExitPossess()
    {
        if (currentToy != null)
        {
            InteractableTag tag = currentToy.GetComponent<InteractableTag>();
            if (tag != null) tag.SetPossessedState(false, possessedHighlightColor);
            currentToy.UnPossess();
        }

        isPossessing = false;
        currentToy = null;
        if (rend)
        {
            rend.enabled = true;
            rend.material.color = normalColor;
        }

        if (audioSrc && possessExitSound) audioSrc.PlayOneShot(possessExitSound);
        targetFOV = normalFOV;
        ClearHover();
        DetectHoverObject();
        // Debug.Log("[Player] Exited possession");
    }
}