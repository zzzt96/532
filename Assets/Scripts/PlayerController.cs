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

    [Header("Ghost-Based Detection")]
    public float possessRadius = 1.5f;

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
    public float ghostZOffset = -1.5f;
    public float normalFOV = 60f;
    public float possessFOV = 35f;
    public float fovSmoothSpeed = 5f;

    [Header("Ghost Juice (Visual Effects)")]
    public bool enableJuice = true;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 2.5f;
    public float scaleAmplitude = 0.05f;
    public float scaleFrequency = 2f;

    [Header("Audio")]
    public AudioClip possessEnterSound;
    public AudioClip possessExitSound;

    // 供 CutsceneManager 调用：过场期间锁住玩家输入
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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

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

        // 过场动画期间锁定玩家
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

            if (Input.GetKeyDown(possessKey)) ExitPossess();
            return;
        }

        // 自由移动状态
        HandleMouseMovement();

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

        HandlePossessInput();
    }

    void HandleMouseMovement()
    {
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

            Vector3 newPos = Vector3.Lerp(logicalPos, targetPos, Time.deltaTime * mouseFollowSmoothing);

            currentBobOffset = enableJuice ? Mathf.Sin(Time.time * bobFrequency) * bobAmplitude : 0f;

            newPos.y += currentBobOffset;
            newPos.z = transform.position.z;
            transform.position = newPos;
        }
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
            if (Mathf.Abs(newTargetZ - targetZ) > 0.1f)
                Debug.Log($"[Player] Z changing: {targetZ:F2} → {newTargetZ:F2} (Hover: {currentHover.name})");
            targetZ = newTargetZ;
        }
        else
        {
            if (Mathf.Abs(defaultZ - targetZ) > 0.1f)
                Debug.Log($"[Player] Z returning to default: {targetZ:F2} → {defaultZ:F2}");
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

            float dist2D;
            if (toy.useXOnlyDetection)
            {
                // TutorialToy在XZ平面移动，只比较X轴距离
                dist2D = Mathf.Abs(transform.position.x - toy.transform.position.x);
            }
            else
            {
                Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);
                Vector2 toyXY   = new Vector2(toy.transform.position.x, toy.transform.position.y);
                dist2D = Vector2.Distance(ghostXY, toyXY);
            }

            if (dist2D <= possessRadius)
                availableToys.Add(toy);
        }

        // XY距离 + Z惩罚排序
        availableToys.Sort((a, b) =>
        {
            Vector2 ghostXY = new Vector2(transform.position.x, transform.position.y);

            float xyA = Vector2.Distance(ghostXY, new Vector2(a.transform.position.x, a.transform.position.y));
            float xyB = Vector2.Distance(ghostXY, new Vector2(b.transform.position.x, b.transform.position.y));

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
        if (currentHover == null || !currentHover.canBePossessed) return;

        // 即时附身，按一下 Shift 立刻触发，暂时不加pending UI
        if (Input.GetKeyDown(possessKey))
            EnterPossess();
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
        Debug.Log($"[Player] Possessed {currentToy.name}");

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

        if (audioSrc && possessExitSound) audioSrc.PlayOneShot(possessExitSound);

        targetFOV = normalFOV;
        Debug.Log("[Player] Exited possession");
    }
}