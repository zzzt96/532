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

    // 记录是否正在被玩家附身（由 PlayerController 控制）
    [HideInInspector] public bool isCurrentlyPossessed = false;

    public static readonly List<InteractableTag> All = new List<InteractableTag>();

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
        if (highlightEffect != null) highlightEffect.SetHighlighted(false);
    }

    // 新增：由 PlayerController 调用，强制锁定或解锁附身的高亮状态
    public void SetPossessedState(bool state, Color possessColor)
    {
        isCurrentlyPossessed = state;

        if (highlightEffect == null) return;

        if (state)
        {
            highlightEffect.outlineColor = possessColor;
            highlightEffect.SetHighlighted(true);
        }
        else
        {
            // 解除附身时，先把它关掉，下一帧如果玩家还按着Q/E，TabHighlighter会重新接管它
            highlightEffect.SetHighlighted(false);
        }
    }

    // 接收管理器传来的按键状态和颜色
    public void UpdateHighlight(bool isQPressed, bool isEPressed, Color possessColor, Color interactColor)
    {
        if (highlightEffect == null) return;

        // 【核心修改】：如果正在被附身，直接无视按键状态，保持常亮，不执行下面的逻辑
        if (isCurrentlyPossessed) return;

        bool shouldShowPossess = isQPressed && canBePossessed;
        bool shouldShowInteract = isEPressed && isInteractable;

        if (shouldShowPossess || shouldShowInteract)
        {
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
            highlightEffect.SetHighlighted(false);
        }
    }
}