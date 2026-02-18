using UnityEngine;
using System.Collections.Generic;
using HighlightPlus;

public class InteractableTag : MonoBehaviour
{
    [Header("状态配置 (多选一或都选)")]
    public bool canBePossessed = false;
    public bool isInteractable = true;

    [Header("必须拖入该物体上的 Highlight Effect 组件")]
    public HighlightEffect highlightEffect;

    // 静态注册表，供高亮管理器全局调用
    public static readonly List<InteractableTag> All = new List<InteractableTag>();

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
        // 物体失活时确保关掉高亮，防止残留
        if (highlightEffect != null) highlightEffect.SetHighlighted(false);
    }

    // 接收管理器传来的颜色并执行高亮
    public void SetHighlight(bool state, Color possessColor, Color interactColor)
    {
        if (highlightEffect == null) return;

        if (state)
        {
            // 优先级判断：如果又能附身又能交互，优先显示附身颜色（你可以根据需求调换顺序）
            if (canBePossessed)
            {
                highlightEffect.outlineColor = possessColor;
            }
            else if (isInteractable)
            {
                highlightEffect.outlineColor = interactColor;
            }

            highlightEffect.SetHighlighted(true);
        }
        else
        {
            highlightEffect.SetHighlighted(false);
        }
    }
}