using UnityEngine;
using System.Collections;

/// <summary>
/// 摇椅 - 被碰撞物体撞到后开始晃动
/// 晃动一段时间后吸引猫过来
/// </summary>
public class RockingChair : MonoBehaviour
{
    [Header("Rocking")]
    public float rockAngle = 15f;
    public float rockSpeed = 2.5f;
    [Tooltip("晃多少秒后猫才被吸引过来")]
    public float rockDuration = 2.5f;

    private bool isRocking = false;

    // 被物理碰撞触发（Fan吹倒的物体撞过来）
    void OnCollisionEnter(Collision col)
    {
        if (!isRocking) StartRocking();
    }

    /// <summary>也可由外部直接调用</summary>
    public void StartRocking()
    {
        if (isRocking) return;
        isRocking = true;
        StartCoroutine(RockRoutine());
    }

    IEnumerator RockRoutine()
    {
        float elapsed = 0f;
        while (elapsed < rockDuration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * rockSpeed * Mathf.PI) * rockAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        isRocking = false;

        Debug.Log("[RockingChair] Rocking attracted the cat!");
        // 让猫过来（Level2Manager 持有猫的引用）
        Level2Manager.Instance?.cat?.GoToRockingChair();
    }
}