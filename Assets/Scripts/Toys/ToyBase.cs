using UnityEngine;

/// <summary>
/// 可交互物体基类
/// 所有可交互物体（电灯、网球、篮球等）都继承这个类
/// </summary>
public abstract class ToyBase : MonoBehaviour
{
    [Header("Possession")]
    public bool canBePossessed = true; // 是否可被附身
    public bool isPossessed = false;   // 是否正在被附身

    [Header("Visual Feedback")]
    public Color hoverColor = Color.yellow;
    public Color possessColor = Color.cyan;

    protected Rigidbody rb;
    protected Renderer rend;
    protected Color originalColor;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        
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