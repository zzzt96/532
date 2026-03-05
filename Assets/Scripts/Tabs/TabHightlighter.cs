using UnityEngine;

public class TabHighlighter : MonoBehaviour
{
    [Header("按键设置")]
    public KeyCode possessKey = KeyCode.Q;  // 看附身物体的按键
    public KeyCode interactKey = KeyCode.E; // 看交互物体的按键

    [Header("颜色配置")]
    public Color possessableColor = Color.yellow; // 黄色
    public Color interactableColor = Color.cyan;  // 蓝色 (用 Cyan 比纯蓝在暗背景下更清晰)

    [Header("可选：只高亮一定范围内的物体")]
    public float maxDistance = 15f;
    public Transform playerTransform; // 如果为空，默认使用主摄像机位置

    // 记录两个按键当前是否被按下
    private bool isQPressed = false;
    private bool isEPressed = false;

    void Start()
    {
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        bool stateChanged = false;

        // 检测 Q 键的长按状态
        if (Input.GetKeyDown(possessKey)) { isQPressed = true; stateChanged = true; }
        else if (Input.GetKeyUp(possessKey)) { isQPressed = false; stateChanged = true; }

        // 检测 E 键的长按状态
        if (Input.GetKeyDown(interactKey)) { isEPressed = true; stateChanged = true; }
        else if (Input.GetKeyUp(interactKey)) { isEPressed = false; stateChanged = true; }

        // 只有在按键状态发生变化时，才去遍历所有物体更新高亮，节省性能
        if (stateChanged)
        {
            UpdateAllHighlights();
        }
    }

    void UpdateAllHighlights()
    {
        Vector3 checkPosition = playerTransform != null ? playerTransform.position : Vector3.zero;

        foreach (var tag in InteractableTag.All)
        {
            if (tag == null) continue;

            // 检查距离
            float distance = Vector3.Distance(checkPosition, tag.transform.position);

            if (distance <= maxDistance)
            {
                // 在范围内，传入当前的按键状态让标签自己判断
                tag.UpdateHighlight(isQPressed, isEPressed, possessableColor, interactableColor);
            }
            else
            {
                // 超出范围，直接强行传 false 关掉高亮
                tag.UpdateHighlight(false, false, possessableColor, interactableColor);
            }
        }
    }
}