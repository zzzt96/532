using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector2 startPosition = new Vector2(0, 1);

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

    [Header("Ghost Visibility")]
    public float ghostZOffset = -1.5f;  // 幽灵 Z 偏移（负值 = 靠近相机 = 不被遮挡）
    public float normalFOV = 60f;
    public float possessFOV = 35f;
    public float fovSmoothSpeed = 5f;

    [Header("Ghost Juice (Visual Effects)")]
    public bool enableJuice = true;
    public float bobAmplitude = 0.3f;    // 上下浮动的幅度
    public float bobFrequency = 2.5f;    // 上下浮动的频率
    public float scaleAmplitude = 0.05f; // 呼吸缩放的比例 (0.05 = 5%)
    public float scaleFrequency = 2f;    // 呼吸缩放的频率

    [Header("Audio")]
    public AudioClip possessEnterSound; // 附身时的音效
    public AudioClip possessExitSound;  // 离开附身时的音效

    private Camera mainCam;
    private Renderer rend;
    private List<ToyBase> availableToys = new List<ToyBase>();
    private int currentToyIndex = 0;
    private float targetZ;
    private float targetFOV;
    private AudioSource audioSrc;

    // Juice internal variables
    private Vector3 originalScale;
    private float currentBobOffset = 0f;

    void Start()
    {
        // 获取自身的 AudioSource
        audioSrc = GetComponent<AudioSource>();

        // 1. 隐藏系统鼠标指针
        Cursor.visible = false;

        // 2. 将鼠标限制在游戏窗口内（推荐！）
        // 防止玩家猛甩鼠标时点到游戏外面的桌面或其他软件
        Cursor.lockState = CursorLockMode.Confined;

        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = normalColor;

        transform.position = new Vector3(startPosition.x, startPosition.y, defaultZ + ghostZOffset);
        targetZ = defaultZ;

        // 初始化 FOV
        targetFOV = normalFOV;
        if (mainCam) mainCam.fieldOfView = normalFOV;

        // 记录初始缩放用于表现恢复
        originalScale = transform.localScale;
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

        // 平滑过渡 FOV
        if (mainCam && Mathf.Abs(mainCam.fieldOfView - targetFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
        }

        if (isPossessing && currentToy != null)
        {
            // 保留队友的 cameraYOffset 逻辑
            transform.position = currentToy.transform.position + new Vector3(0, currentToy.cameraYOffset, 0);
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
                pos.z = defaultZ + ghostZOffset;
                transform.position = pos;
            }

            // 在核心位移逻辑完成后，叠加视觉果汁表现
            ApplyGhostJuice();
        }
    }

    void HandleMouseMovement()
    {
        // 1. 剔除上一帧的浮动偏移，获取鬼魂的"真实逻辑位置"
        Vector3 logicalPos = transform.position;
        logicalPos.y -= currentBobOffset;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
            float targetY = Mathf.Clamp(mouseWorldPos.y, minY, maxY);
            Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);

            // 2. 使用逻辑位置进行插值跟随 (替换队友原来的直接插值)
            Vector3 newPos = Vector3.Lerp(logicalPos, targetPos, Time.deltaTime * mouseFollowSmoothing);

            // 3. 计算这一帧的新浮动偏移
            currentBobOffset = enableJuice ? Mathf.Sin(Time.time * bobFrequency) * bobAmplitude : 0f;

            // 4. 将新的偏移加回去，完成最终定位
            newPos.y += currentBobOffset;
            newPos.z = transform.position.z; // 保持Z轴逻辑不变
            transform.position = newPos;
        }
    }

    void ApplyGhostJuice()
    {
        if (!enableJuice) return;

        // 呼吸感缩放 (Breathing)
        float scaleOffset = Mathf.Sin(Time.time * scaleFrequency) * scaleAmplitude;
        transform.localScale = originalScale * (1f + scaleOffset);
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
        // 保留队友加入的 ghostZOffset
        pos.z = Mathf.Lerp(pos.z, targetZ + ghostZOffset, Time.deltaTime * zSmoothSpeed);
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

        // 播放附身进入音效
        if (audioSrc && possessEnterSound) audioSrc.PlayOneShot(possessEnterSound);

        // Zoom in
        targetFOV = possessFOV;
        Debug.Log($"[Player] Possessed {currentToy.name}, zooming in to FOV={possessFOV}");

        // 附身时重置缩放和偏移
        transform.localScale = originalScale;
        currentBobOffset = 0f;
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

        // 播放附身退出音效
        if (audioSrc && possessExitSound) audioSrc.PlayOneShot(possessExitSound);

        // Zoom out
        targetFOV = normalFOV;
        Debug.Log("[Player] Exited possession, zooming out to FOV=" + normalFOV);
    }
}