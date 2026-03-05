using UnityEngine;

public class TabHighlighter : MonoBehaviour
{
    [Header("按键设置")]
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("颜色配置")]
    public Color possessableColor = new Color(0.8f, 0.6f, 0.0f); // 橙色
    public Color interactableColor = new Color(0.2f, 0.8f, 0.2f); // 绿色

    [Header("可选：只高亮一定范围内的物体")]
    public float maxDistance = 15f;
    public Transform playerTransform; // 如果为空，默认使用主摄像机位置

    void Start()
    {
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 按下时亮起
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleAll(true);
        }
        // 松开时熄灭（如果你想做成按一下开、再按一下关的开关模式，这里逻辑可以稍微修改）
        else if (Input.GetKeyUp(toggleKey))
        {
            ToggleAll(false);
        }
    }

    void ToggleAll(bool state)
    {
        Vector3 checkPosition = playerTransform != null ? playerTransform.position : Vector3.zero;

        foreach (var tag in InteractableTag.All)
        {
            if (tag == null) continue;

            // 如果是开启状态，检查距离
            if (state)
            {
                float distance = Vector3.Distance(checkPosition, tag.transform.position);
                if (distance <= maxDistance)
                {
                    // 在范围内，传入对应的颜色并开启
                    tag.SetHighlight(true, possessableColor, interactableColor);
                }
            }
            else
            {
                // 如果是关闭状态，直接无视距离全部关闭
                tag.SetHighlight(false, possessableColor, interactableColor);
            }
        }
    }
}