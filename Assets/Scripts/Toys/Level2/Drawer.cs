using UnityEngine;
using System.Collections;

/// <summary>
/// 抽屉 - 由 CatNPC.OpenDrawerRoutine() 调用 Open()
/// 打开完成后通知 Level2Manager，镜子自动转向衣柜
/// </summary>
public class Drawer : MonoBehaviour
{
    [Header("Open Animation")]
    [Tooltip("抽屉打开方向和距离，通常 X 轴正方向")]
    public Vector3 openOffset = new Vector3(0.6f, 0f, 0f);
    public float openDuration = 0.4f;

    private bool isOpen = false;
    private Vector3 closedLocalPos;

    void Awake()
    {
        closedLocalPos = transform.localPosition;
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        Vector3 openLocalPos = closedLocalPos + openOffset;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            transform.localPosition = Vector3.Lerp(closedLocalPos, openLocalPos, t);
            yield return null;
        }
        transform.localPosition = openLocalPos;

        Debug.Log("[Drawer] Opened! Notifying Level2Manager.");
        Level2Manager.Instance?.OnDrawerOpened();
    }
}