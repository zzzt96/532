using UnityEngine;

/// <summary>
/// 天窗手柄开关 - 玩家附身后按 E/Space 打开天窗
/// 触发：猫走向光斑 + 女孩开始自动移动
/// </summary>
public class SkyLightSwitch : ToyBase
{
    [Header("Skylight Visuals")]
    [Tooltip("天窗板子的GameObject（打开时激活动画或SetActive）")]
    public GameObject skylightPanel;
    [Tooltip("天窗打开后出现的光柱/光斑")]
    public GameObject sunlightBeam;

    private bool isOpen = false;

    public override void ToyUpdate()
    {
        if (isOpen) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            OpenSkylight();
    }

    void OpenSkylight()
    {
        isOpen = true;
        if (skylightPanel != null) skylightPanel.SetActive(true);
        if (sunlightBeam  != null) sunlightBeam.SetActive(true);

        GetComponent<InteractableTag>()?.SetCompleted();
        canBePossessed = false;

        Debug.Log("[SkyLight] Opened!");
        Level2Manager.Instance?.OnSkylightOpened();
    }
}