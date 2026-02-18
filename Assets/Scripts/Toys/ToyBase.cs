using UnityEngine;

/// <summary>
/// 可交互物体基类
/// </summary>
public abstract class ToyBase : MonoBehaviour
{
    [Header("Possession")]
    public bool canBePossessed = true;
    public bool isPossessed = false;

    [Header("Visual Feedback")]
    public Color hoverColor = Color.yellow;
    public Color possessColor = Color.cyan;

    [Header("Camera")]
    public float cameraYOffset = 0f;

    protected Rigidbody rb;
    protected Renderer rend;
    protected Color originalColor;

    // [新增] 声音组件，供所有子类使用
    protected AudioSource audioSrc;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        audioSrc = GetComponent<AudioSource>(); // 获取自身挂载的AudioSource

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
}