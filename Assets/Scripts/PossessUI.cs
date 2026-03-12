using UnityEngine;
using UnityEngine.UI;

public class PossessUI : MonoBehaviour
{
    public static PossessUI Instance;

    [Header("References")]
    public Image circleImage;       // 圆圈填充 Image，Image Type = Filled, Fill Method = Radial 360
    public RectTransform uiRoot;    // 圆圈的根物体

    [Header("Settings")]
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0); // 圆圈在物体上方的偏移

    private Camera mainCam;

    void Awake()
    {
        Instance = this;
        if (uiRoot) uiRoot.gameObject.SetActive(false);
    }

    void Start()
    {
        mainCam = Camera.main;
    }

    public void Show(Vector3 worldPos, float progress)
    {
        if (uiRoot == null || circleImage == null) return;

        uiRoot.gameObject.SetActive(true);

        // 跟随世界坐标
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos + worldOffset);
        uiRoot.position = screenPos;

        // 更新进度
        circleImage.fillAmount = progress;
    }

    public void Hide()
    {
        if (uiRoot) uiRoot.gameObject.SetActive(false);
        if (circleImage) circleImage.fillAmount = 0f;
    }
}