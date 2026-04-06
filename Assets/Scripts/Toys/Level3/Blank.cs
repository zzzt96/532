using System.Collections;
using UnityEngine;

/// <summary>
/// 木板
/// 被桌子震动触发后，倒下盖住水坑
/// 倒下方向：绕Z轴旋转（向左倒）
/// </summary>
public class Blank : MonoBehaviour
{
    [Header("Fall Settings")]
    [Tooltip("倒下方向角度（绕Z轴，正值=向左倒，负值=向右倒）")]
    public float fallAngle = -85f;
    [Tooltip("倒下时的X位移（落到水坑上）")]
    public float fallSlideX = -1f;
    [Tooltip("倒下后Y轴下移距离（贴地）")]
    public float fallDropY = 0.5f;
    [Tooltip("倒下后Z轴位移（往镜头靠近为负值）")]
    public float fallSlideZ = -0.5f;
    public float fallDuration = 0.6f;

    Quaternion startRot;
    Vector3 startPos;

    void Start()
    {
        startRot = transform.localRotation;
        startPos = transform.localPosition;
    }

    public void TriggerFall()
    {
        StartCoroutine(FallRoutine());
    }

    IEnumerator FallRoutine()
    {
        Quaternion endRot = Quaternion.Euler(0f, 0f, fallAngle) * startRot;
        Vector3 endPos = startPos + new Vector3(fallSlideX, -fallDropY, fallSlideZ);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out

            transform.localRotation = Quaternion.Slerp(startRot, endRot, eased);
            transform.localPosition = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.localRotation = endRot;
        transform.localPosition = endPos;

        Debug.Log("[Blank] Plank fell, puddle covered.");
        Level3Manager.Instance?.OnPlankFell();
    }
}