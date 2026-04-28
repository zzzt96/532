using UnityEngine;
using System.Collections;

public class SkyLightSwitch : ToyBase
{
    [Header("Skylight Visuals")]
    public GameObject skylightPanel;
    public GameObject sunlightBeam;

    [Header("Open Animation")]
    public float openAngleX = -90f;
    public float openDuration = 0.6f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("把手旋转的金属/木质咔哒+旋转声 (按下 Space 那一瞬间)")]
    public SoundSlot handleRotateSound;

    [Tooltip("窗户打开的老旧结构咯吱声 (开窗动画期间)")]
    public SoundSlot skylightOpenSound;
    // ===============================================

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

        // 把手旋转音效 (按下瞬间)
        PlaySound(handleRotateSound);

        // 短暂等待让把手声先响, 再播窗户咯吱声
        yield return new WaitForSeconds(0.15f);

        // 窗户咯吱开启音效
        PlaySound(skylightOpenSound);

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

        if (sunlightBeam != null) sunlightBeam.SetActive(true);

        Debug.Log("[SkyLight] Opened!");
        Level2Manager.Instance?.OnSkylightOpened();

        // zoom out 修复: 天窗打开后玩家退出附身, 看猫和小女孩开始动作
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[SkyLight] Auto-exited possession.");
        }
    }
}