using UnityEngine;

public abstract class ToyBase : MonoBehaviour
{
    [Header("Possession")]
    public bool canBePossessed = true;
    public bool isPossessed = false;
    public bool useXOnlyDetection = false; // TutorialToy专用，勾选后只比较X轴距离
    
    [Header("Visual Feedback")]
    public Color hoverColor = Color.yellow;
    public Color possessColor = Color.cyan;

    [Header("Camera")]
    public float cameraYOffset = 0f;

    protected Rigidbody rb;
    protected Renderer rend;
    protected Color originalColor;
    
    [Tooltip("检测位置偏移，用于高处物体把检测点下移")] // Level 3专用
    public Vector3 detectionOffset = Vector3.zero;
    
    [Tooltip("附身UI显示位置的偏移（用于高处物体把UI下移到玩家面前）")] // Level 3专用
    public Vector3 uiOffset = Vector3.zero;

    // [新增] 声音组件，供所有子类使用
    protected AudioSource audioSrc;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        audioSrc = GetComponent<AudioSource>();

        if (rend && rend.material)
            originalColor = rend.material.color;
    }

    /// <summary>
    /// 玩家鼠标悬停在物体上时调用
    /// </summary>
    public virtual void OnHoverEnter()
    {
        if (rend && canBePossessed)
            rend.material.color = hoverColor;
    }

    /// <summary>
    /// 玩家鼠标离开物体时调用
    /// </summary>
    public virtual void OnHoverExit()
    {
        if (rend && !isPossessed)
            rend.material.color = originalColor;
    }

    /// <summary>
    /// 玩家附身时调用
    /// </summary>
    public virtual void Possess()
    {
        isPossessed = true;
        if (rend)
            rend.material.color = possessColor;
    }

    /// <summary>
    /// 玩家脱离附身时调用
    /// </summary>
    public virtual void UnPossess()
    {
        isPossessed = false;
        if (rend)
            rend.material.color = originalColor;
    }

    /// <summary>
    /// 每帧更新，只在被附身时调用
    /// 子类重写这个方法实现具体的交互逻辑
    /// </summary>
    public abstract void ToyUpdate();
    
    
    // ==================== Audio System (通用音效系统) ====================
    // 所有子类都可以直接调用 PlaySound(soundSlot) / StopSound() 来触发音效，
    protected void PlaySound(SoundSlot slot)
    {
        if (slot == null || !slot.IsValid()) return;
        if (audioSrc == null) return;

        if (slot.loop)
        {
            // 循环音效：如果当前已经在播同一个 clip，就不要重启（避免抖动）
            if (audioSrc.isPlaying && audioSrc.clip == slot.clip) return;

            audioSrc.clip = slot.clip;
            audioSrc.volume = slot.volume;
            audioSrc.pitch = slot.GetPitch();
            audioSrc.loop = true;
            audioSrc.Play();
        }
        else
        {
            // 单次音效：用 PlayOneShot 不会打断其他正在播放的循环音
            audioSrc.pitch = slot.GetPitch();
            audioSrc.PlayOneShot(slot.clip, slot.volume);
        }
    }
    
    protected void StopSound()
    {
        if (audioSrc == null) return;
        if (audioSrc.isPlaying && audioSrc.loop)
        {
            audioSrc.Stop();
            audioSrc.loop = false;
        }
    }
}