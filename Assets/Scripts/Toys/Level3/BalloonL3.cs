using System.Collections;
using UnityEngine;

/// <summary>
/// 气球
/// 由AirPump充满后：
/// 1. 切换小气球→大气球
/// 2. 书本被顶开（向上+旋转倒落）
/// 3. 台灯Light intensity亮起
/// </summary>
public class BalloonL3 : MonoBehaviour
{
    [Header("Balloon Objects")]
    [Tooltip("小气球GameObject（初始显示）")]
    public GameObject balloonSmall;
    [Tooltip("大气球GameObject（充满后显示）")]
    public GameObject balloonLarge;

    [Header("Books")]
    public Transform books;
    [Tooltip("书本先向上顶多少")]
    public float booksLiftY = 0.5f;
    [Tooltip("书本最终落地的localPosition（相对父物体）")]
    public Vector3 booksFinalLocalPos = new Vector3(0f, -1f, 0f);
    [Tooltip("书本躺倒的最终旋转角（绕Z轴）")]
    public float booksFallAngle = 90f;
    [Tooltip("上顶动画时间")]
    public float booksLiftDuration = 0.2f;
    [Tooltip("落地动画时间")]
    public float booksFallDuration = 0.5f;

    [Header("Desk Lamp")]
    [Tooltip("台灯的Light组件")]
    public Light deskLampLight;
    [Tooltip("台灯亮起的intensity")]
    public float lampIntensity = 3f;

    void Start()
    {
        // 初始显示小气球，隐藏大气球
        if (balloonSmall != null) balloonSmall.SetActive(true);
        if (balloonLarge != null) balloonLarge.SetActive(false);
        if (deskLampLight != null) deskLampLight.intensity = 0f;
    }

    /// <summary>由Level3Manager调用</summary>
    public void TriggerInflate()
    {
        StartCoroutine(InflateSequence());
    }

    IEnumerator InflateSequence()
    {
        if (balloonSmall != null) balloonSmall.SetActive(false);
        if (balloonLarge != null) balloonLarge.SetActive(true);
        Debug.Log("[BalloonL3] Balloon inflated!");

        yield return new WaitForSeconds(0.3f);

        if (books != null)
            yield return StartCoroutine(LiftBooks());

        yield return new WaitForSeconds(0.2f);

        if (deskLampLight != null)
        {
            deskLampLight.intensity = lampIntensity;
            Debug.Log("[BalloonL3] Desk lamp on!");
        }
        Level3Manager.Instance?.OnDeskLampOn();
    }

    IEnumerator LiftBooks()
    {
        Vector3 startPos = books.localPosition;
        Quaternion startRot = books.localRotation;
        Vector3 liftPos = startPos + Vector3.up * booksLiftY;

        // 第一段：向上顶
        float elapsed = 0f;
        while (elapsed < booksLiftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / booksLiftDuration);
            books.localPosition = Vector3.Lerp(startPos, liftPos, t);
            yield return null;
        }

        // 第二段：落地+旋转躺倒
        Quaternion fallRot = Quaternion.Euler(0f, 0f, booksFallAngle) * startRot;
        elapsed = 0f;
        while (elapsed < booksFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / booksFallDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            books.localPosition = Vector3.Lerp(liftPos, booksFinalLocalPos, eased);
            books.localRotation = Quaternion.Slerp(startRot, fallRot, eased);
            yield return null;
        }
        books.localPosition = booksFinalLocalPos;
        books.localRotation = fallRot;
        Debug.Log("[BalloonL3] Books fell to ground!");
    }
}