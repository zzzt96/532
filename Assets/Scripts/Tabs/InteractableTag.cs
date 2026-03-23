using UnityEngine;
using System.Collections.Generic;
using HighlightPlus;

public class InteractableTag : MonoBehaviour
{
    [Header("State")]
    public bool canBePossessed = false;
    public bool isPossessableObject = false;

    [Header("Highlight Effect")]
    public HighlightEffect highlightEffect;

    [HideInInspector] public bool isCurrentlyPossessed = false;
    [HideInInspector] public bool isCompleted = false;

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

    public void SetCompleted()
    {
        isCompleted = true;
        canBePossessed = false;
    
        // 同步ToyBase(由PlayerController判断)，阻止玩家实际附身，和这个脚本里的InteractableTag.canBePossessed是两个独立字段
        ToyBase toy = GetComponent<ToyBase>();
        if (toy != null) toy.canBePossessed = false;
        if (highlightEffect != null) highlightEffect.SetHighlighted(false);
    }
    
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
            highlightEffect.SetHighlighted(false);
        }
    }

    public void UpdateHighlight(bool isQPressed, Color possessColor, Color lockedColor)
    {
        if (highlightEffect == null) return;
        if (isCurrentlyPossessed) return;

        if (isCompleted || !isQPressed)
        {
            highlightEffect.SetHighlighted(false);
            return;
        }

        if (canBePossessed)
        {
            highlightEffect.outlineColor = possessColor;
            highlightEffect.SetHighlighted(true);
        }
        else if (isPossessableObject)
        {
            highlightEffect.outlineColor = lockedColor;
            highlightEffect.SetHighlighted(true);
        }
        else
        {
            highlightEffect.SetHighlighted(false);
        }
    }
}