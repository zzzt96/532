using UnityEngine;
using System.Collections;

/// <summary>
/// 天窗手柄开关 - 玩家附身后按 space 打开天窗
/// 触发：猫走向光斑 + 女孩开始自动移动
/// </summary>
public class SkyLightSwitch : ToyBase
{
    [Header("Skylight Visuals")]
    public GameObject skylightPanel;
    public GameObject sunlightBeam;

    [Header("Open Animation")]
    public float openAngleX = -90f;
    public float openDuration = 0.6f;

    private bool isOpen = false;

    public override void ToyUpdate()
    {
        if (isOpen) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            StartCoroutine(OpenSkylight());
    }

    IEnumerator OpenSkylight()
    {
        isOpen = true;
        canBePossessed = false;
        GetComponent<InteractableTag>()?.SetCompleted();

        // 天窗旋转打开
        if (skylightPanel != null)
        {
            skylightPanel.SetActive(true);
            Quaternion startRot = skylightPanel.transform.localRotation;
            Quaternion endRot = Quaternion.Euler(openAngleX,
                skylightPanel.transform.localEulerAngles.y,
                skylightPanel.transform.localEulerAngles.z);

            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
                skylightPanel.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }
            skylightPanel.transform.localRotation = endRot;
        }

        // 光柱出现
        if (sunlightBeam != null) sunlightBeam.SetActive(true);

        Debug.Log("[SkyLight] Opened!");
        Level2Manager.Instance?.OnSkylightOpened();
    }
}