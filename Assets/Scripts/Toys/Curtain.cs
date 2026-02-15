using UnityEngine;

public class Curtain : MonoBehaviour
{
    [Header("Curtain Object")]
    public Transform curtainMask;       // 窗帘本体（会被移动/缩放来表示打开）

    [Header("Open Method")]
    public bool useScale = true;        // true=缩放Y打开, false=向上移动打开
    public float moveUpDistance = 3f;   // useScale=false时，窗帘往上移多远

    [Header("State")]
    public float openProgress = 0f;

    private Vector3 curtainStartPos;
    private Vector3 curtainStartScale;

    void Start()
    {
        if (curtainMask != null)
        {
            curtainStartPos = curtainMask.localPosition;
            curtainStartScale = curtainMask.localScale;
        }
    }

    /// <summary>
    /// 设置窗度 0=帘打开程关 1=完全打开帘打开程
    /// </summary>
    public void SetOpenProgress(float progress)
    {
        openProgress = Mathf.Clamp01(progress);

        if (curtainMask == null) return;

        if (useScale)
        {
            // 缩放方式：Y从1缩到0
            Vector3 scale = curtainStartScale;
            scale.y = Mathf.Lerp(curtainStartScale.y, 0f, openProgress);
            curtainMask.localScale = scale;
        }
        else
        {
            // 移动方式：窗帘往上收
            Vector3 pos = curtainStartPos;
            pos.y += openProgress * moveUpDistance;
            curtainMask.localPosition = pos;
        }
    }

    public void Pull(float amount)
    {
        SetOpenProgress(openProgress + amount);
    }
}