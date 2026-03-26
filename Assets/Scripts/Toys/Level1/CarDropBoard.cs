using UnityEngine;
using System.Collections;

public class CarDropBoard : MonoBehaviour
{
    [Header("References")]
    public GameObject toyCar;
    public Transform boardMesh;         // 木板的视觉Mesh物体（做旋转动画用）

    [Header("Tip Animation")]
    public float tipDuration = 0.5f;    // 木板翻倒时间
    public float tipAngle = 80f;        // 翻倒角度
    public Vector3 tipAxis = Vector3.forward; // 绕哪个轴翻（根据木板朝向调整）

    [Header("State")]
    public bool hasTriggered = false;

    // 被猫触发
    public void TipBoard()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(TipAndSlide());
        Debug.Log("[CarDropBoard] Board tipping!");
    }

    // 旧接口保留兼容
    public void TriggerDrop() { TipBoard(); }
    public void TriggerDrop(float force) { TipBoard(); }

    IEnumerator TipAndSlide()
    {
        // 木板翻倒动画 
        if (boardMesh != null)
        {
            Quaternion startRot = boardMesh.rotation;
            Quaternion endRot = startRot * Quaternion.AngleAxis(tipAngle, tipAxis);
            float elapsed = 0f;

            while (elapsed < tipDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / tipDuration;
                float smooth = t * t * (3f - 2f * t);
                boardMesh.rotation = Quaternion.Lerp(startRot, endRot, smooth);
                yield return null;
            }
            boardMesh.rotation = endRot;
        }

        // 激活小车（开始滑落）
        if (toyCar != null)
        {
            ToyCar car = toyCar.GetComponent<ToyCar>();
            if (car != null)
            {
                car.SlideOffBoard();
                Debug.Log("[CarDropBoard] Car sliding off!");
            }
        }
    }
}