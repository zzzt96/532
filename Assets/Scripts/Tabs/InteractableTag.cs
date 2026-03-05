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

    // 接收管理器传来的按键状态和颜色
    public void UpdateHighlight(bool isQPressed, bool isEPressed, Color possessColor, Color interactColor)
    {
        if (highlightEffect == null) return;

        // 判断当前物体是否满足亮起的条件
        bool shouldShowPossess = isQPressed && canBePossessed;
        bool shouldShowInteract = isEPressed && isInteractable;

        if (shouldShowPossess || shouldShowInteract)
        {
            // 优先级判断：如果同时按下了Q和E，且这个物体刚好既能附身又能交互，优先显示黄色(附身)
            if (shouldShowPossess)
            {
                highlightEffect.outlineColor = possessColor;
            }
            else if (shouldShowInteract)
            {
                highlightEffect.outlineColor = interactColor;
            }

            highlightEffect.SetHighlighted(true);
        }
        else
        {
            // 如果都不满足，就熄灭
            highlightEffect.SetHighlighted(false);
        }
    }
}