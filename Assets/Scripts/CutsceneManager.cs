using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Tutorial结束后：
/// 1. 锁住玩家输入
/// 2. 镜头平滑移向小女孩
/// 3. 显示目标提示 UI（"Wake up the little girl!"）
/// 4. 玩家按任意键 → UI淡出 → 镜头归位 → 解锁玩家
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("References")]
    public Transform girlTransform;          
    public CameraFollowSphereOnly camFollow; 
    public PlayerController player;          

    [Header("Camera Pan Settings")]
    public float panSpeed = 3f;            
    public float holdDuration = 1.5f; 
    public float zoomIn = 4f; 
    public Vector3 girlCamOffset = new Vector3(0, 0, 0); // X往左负数，Y高度，Z不用（zoomIn控制）

    [Header("UI")]
    public CanvasGroup objectivePanel;  // 包含 "Wake up the little girl!" 的 CanvasGroup
    public float fadeDuration = 0.5f;
    
    [Header("Dev Tools")]
    #if UNITY_EDITOR
    public bool skipCutscene = false;
    #endif
    
    void Awake()
    {
        Instance = this;
        if (objectivePanel) objectivePanel.alpha = 0f;
    }

    void Start()
    {
    #if UNITY_EDITOR
        if (skipCutscene)
        {
            player.inputLocked = false;
            Debug.Log("[Cutscene] Skipped (Dev Mode)");
            return;
        }
    #endif
        PlayObjectiveCutscene();
    }
    
    public void PlayObjectiveCutscene()
    {
        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        // 1. 锁住玩家 & 关闭摄像机跟随
        player.inputLocked = true;
        camFollow.enabled = false;

        Camera cam = Camera.main;
        Vector3 startCamPos = cam.transform.position;
   
        // 平移到女孩位置，同时Z轴推近（负值 = 靠近女孩）
        Vector3 camToPlayer = cam.transform.position - player.transform.position;
        Vector3 targetCamPos = girlTransform.position + camToPlayer + new Vector3(girlCamOffset.x, girlCamOffset.y, zoomIn);

        // 2. 平滑移向小女孩
        float elapsed = 0f;
        float panDuration = Vector3.Distance(startCamPos, targetCamPos) / panSpeed;
        panDuration = Mathf.Max(panDuration, 0.5f);

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            cam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, elapsed / panDuration);
            yield return null;
        }
        cam.transform.position = targetCamPos;

        // 3. 淡入 UI
        yield return StartCoroutine(FadeUI(0f, 1f));

        // 4. 等待玩家按键（停留 holdDuration 秒后提示按键，或直接等按键）
        float held = 0f;
        bool waitingForInput = false;
        while (true)
        {
            held += Time.deltaTime;
            if (held >= holdDuration) waitingForInput = true;
            if (waitingForInput && Input.anyKeyDown) break;
            yield return null;
        }

        // 5. 淡出 UI
        yield return StartCoroutine(FadeUI(1f, 0f));

        // 6. 重新启用摄像机跟随 & 解锁玩家
        camFollow.enabled = true;
        player.inputLocked = false;

        Debug.Log("[Cutscene] Objective cutscene complete, player unlocked.");
    }

    IEnumerator FadeUI(float from, float to)
    {
        if (objectivePanel == null) yield break;

        float elapsed = 0f;
        objectivePanel.alpha = from;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            objectivePanel.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        objectivePanel.alpha = to;
    }
}