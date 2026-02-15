using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HighlightManager : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode highlightKey = KeyCode.Tab;
    public float highlightDuration = 2f;
    public Color highlightColor = Color.yellow;
    public bool highlightOnStart = true;

    [Header("Target")]
    public Transform interactablesParent;

    // 存储原始颜色，防止重复高亮出问题
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private bool isHighlighting = false;

    void Start()
    {
        if (highlightOnStart)
        {
            Invoke("HighlightAll", 0.5f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(highlightKey) && !isHighlighting)
        {
            HighlightAll();
        }
    }

    void HighlightAll()
    {
        if (interactablesParent == null)
        {
            Debug.LogWarning("[HighlightManager] interactablesParent is null!");
            return;
        }

        StartCoroutine(DoHighlight());
    }

    IEnumerator DoHighlight()
    {
        isHighlighting = true;

        Renderer[] renderers = interactablesParent.GetComponentsInChildren<Renderer>();
        Debug.Log($"[HighlightManager] Highlighting {renderers.Length} objects for {highlightDuration}s");

        // 保存原始颜色并设为高亮
        originalColors.Clear();
        foreach (var rend in renderers)
        {
            if (rend == null) continue;
            originalColors[rend] = rend.material.color;
            rend.material.color = highlightColor;
        }

        // 等待
        yield return new WaitForSeconds(highlightDuration);

        // 恢复原始颜色
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material.color = kvp.Value;
            }
        }

        originalColors.Clear();
        isHighlighting = false;

        Debug.Log("[HighlightManager] Highlight finished, colors restored.");
    }
}