using UnityEngine;

public class Curtain : MonoBehaviour
{
    [Header("Curtain Pivot")]
    public Transform curtainPivotTop;   

    [Header("State")]
    public float openProgress = 0f;     // 0=关, 1=完全打开

    /// <summary>
    /// 设置窗帘打开程度
    /// 通过缩放 pivotTop 的 Y 轴，顶部不动，底部往上缩
    /// </summary>
    public void SetOpenProgress(float progress)
    {
        openProgress = Mathf.Clamp01(progress);

        if (curtainPivotTop == null) return;

        // 缩放 pivot 的 Y：1=完全关闭，0=完全打开
        Vector3 scale = curtainPivotTop.localScale;
        scale.y = 1f - openProgress;
        curtainPivotTop.localScale = scale;

        // Debug.Log($"[Curtain] Open: {openProgress * 100:F0}%");
    }

    public void Pull(float amount)
    {
        SetOpenProgress(openProgress + amount);
    }
}