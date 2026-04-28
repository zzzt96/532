using UnityEngine;
using System.Collections;

public class Fan : ToyBase
{
    [Header("Fan Head")]
    public float headRotateSpeed = 50f;
    public float maxHeadAngle = 200f;

    [Header("Fan Blades")]
    public Transform fanBlades;
    public float bladeSpinSpeed = 360f;
    public Transform fanCenter;

    [Header("Toilet Paper")]
    public GameObject toiletPaper;
    public float blowAngle = -180f;
    public float angleTolerance = 15f;
    public float blowHoldTime = 0.8f;

    [Header("Toilet Paper Landing")]
    public Transform toiletPaperLandTarget;
    public float rollDuration = 0.6f;
    public float fallDuration = 0.4f;

    [Header("Domino Chain")]
    public DominoChain dominoChain;

    [Header("Memory Effect")]
    public MemoryEffect memoryEffect;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("风扇启动时电机嗡嗡渐强声 (TurnOn 被调用时一次性播)")]
    public SoundSlot fanStartSound;

    [Tooltip("风扇运转持续呼呼风声 (建议勾选 Loop, 风扇开启后持续播)")]
    public SoundSlot fanRunningSound;

    [Tooltip("纸筒(厕纸)掉落的中空扑通+滚动声")]
    public SoundSlot paperTubeDropSound;

    [Header("Fan Volume Control")]
    [Range(0f, 1f)]
    [Tooltip("玩家附身风扇时的运转音量 (满音量, 突出主交互)")]
    public float runningVolumePossessed = 1.0f;

    [Range(0f, 1f)]
    [Tooltip("玩家退出附身或吹完厕纸后的运转背景音量 (建议 0.15~0.25, 让其他声音突出)")]
    public float runningVolumeBackground = 0.2f;

    [Tooltip("音量切换的淡入淡出时长 (秒)")]
    public float volumeFadeDuration = 0.5f;
    // ===============================================

    private bool isOn = false;
    private float currentHeadAngle = 0f;
    private bool blowTriggered = false;
    private float blowTimer = 0f;
    private Quaternion initialRotation;

    // 当前风扇 loop 应该播放的目标音量 (附身=高, 退出/吹完=低)
    private bool isInBackgroundVolumeMode = false;
    private Coroutine fadeCoroutine;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.localRotation;
        canBePossessed = false;
    }

    void Update()
    {
        if (isOn && fanBlades != null && fanCenter != null)
            fanBlades.RotateAround(fanCenter.position, Vector3.left, bladeSpinSpeed * Time.deltaTime);
    }

    public override void Possess()
    {
        base.Possess();
        // 玩家附身风扇 -> 运转音切回前景音量
        if (isOn && audioSrc != null && audioSrc.isPlaying)
            FadeRunningVolumeTo(runningVolumePossessed);
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 玩家退出附身 -> 运转音降到背景音量 (淡出, 不立即静音, 风扇还在转)
        if (isOn && audioSrc != null && audioSrc.isPlaying)
            FadeRunningVolumeTo(runningVolumeBackground);
    }

    public override void ToyUpdate()
    {
        if (!isOn || blowTriggered) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        currentHeadAngle += input * headRotateSpeed * Time.deltaTime;
        currentHeadAngle = Mathf.Clamp(currentHeadAngle, -maxHeadAngle, 0f);
        transform.localRotation = initialRotation * Quaternion.Euler(0f, currentHeadAngle, 0f);

        bool aimed = Mathf.Abs(currentHeadAngle - blowAngle) <= angleTolerance;
        blowTimer = aimed ? blowTimer + Time.deltaTime : 0f;

        if (blowTimer >= blowHoldTime)
        {
            blowTriggered = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            StartCoroutine(BlowToiletPaper());
        }
    }

    public void TurnOn()
    {
        isOn = true;
        Debug.Log("[Fan] Turned on!");

        PlaySound(fanStartSound);
        StartCoroutine(StartRunningLoopAfterDelay());
    }

    IEnumerator StartRunningLoopAfterDelay()
    {
        float startSoundLength = 0.5f;
        if (fanStartSound != null && fanStartSound.clip != null)
            startSoundLength = fanStartSound.clip.length;

        yield return new WaitForSeconds(startSoundLength);

        if (!blowTriggered)
        {
            PlaySound(fanRunningSound);
            // 启动时默认音量 = SoundSlot 配置的 volume (Berklee 在 Inspector 里设的值)
            // 没附身就直接进入背景音模式
            if (!isPossessed && audioSrc != null)
                FadeRunningVolumeTo(runningVolumeBackground);
        }
    }

    IEnumerator BlowToiletPaper()
    {
        // zoom out: 触发吹厕纸演出后让玩家退出附身
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[Fan] Auto-exited possession for cinematic.");
        }

        // 吹厕纸事件触发后, 风扇运转声永久降为背景音量
        // (UnPossess 也会触发降低, 这里再加一层保证: 即使玩家还附着也降低)
        if (audioSrc != null && audioSrc.isPlaying)
            FadeRunningVolumeTo(runningVolumeBackground);

        if (toiletPaper == null)
        {
            memoryEffect?.ActivateEffect();
            dominoChain?.StartChain();
            yield break;
        }

        Debug.Log("[Fan] Blowing toilet paper!");

        Vector3 startPos = toiletPaper.transform.position;
        Vector3 landPos = toiletPaperLandTarget != null
            ? toiletPaperLandTarget.position
            : new Vector3(startPos.x - 1.5f, 0.15f, startPos.z);
        Vector3 edgePos = new Vector3(landPos.x, startPos.y, startPos.z);

        // 滚到桌边
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rollDuration);
            toiletPaper.transform.position = Vector3.Lerp(startPos, edgePos, t);
            toiletPaper.transform.Rotate(Vector3.forward, -300f * Time.deltaTime);
            yield return null;
        }
        toiletPaper.transform.position = edgePos;

        // 落到地面
        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            toiletPaper.transform.position = Vector3.Lerp(edgePos, landPos, t);
            yield return null;
        }
        toiletPaper.transform.position = landPos;

        PlaySound(paperTubeDropSound);

        Rigidbody rb = toiletPaper.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

        yield return new WaitForSeconds(0.2f);

        Debug.Log("[Fan] Toilet paper landed! Starting domino chain.");

        if (memoryEffect != null)
        {
            memoryEffect.ActivateEffect();
            Debug.Log("[Fan] 触发了风扇回忆特效！");
        }

        dominoChain?.StartChain();
    }

    /// <summary>
    /// 平滑过渡运转音量到目标值 (淡入淡出)。
    /// </summary>
    void FadeRunningVolumeTo(float targetVolume)
    {
        if (audioSrc == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(targetVolume));
    }

    IEnumerator FadeRoutine(float targetVolume)
    {
        float startVolume = audioSrc.volume;
        float elapsed = 0f;

        while (elapsed < volumeFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / volumeFadeDuration;
            audioSrc.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        audioSrc.volume = targetVolume;
        fadeCoroutine = null;
    }
}