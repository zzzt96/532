using UnityEngine;
using System.Collections;

/// <summary>
/// 木箱+相册 - 猫跳上来后相册掉落
/// 相册落地 → 女孩走过来捡起 → 关卡结束
/// </summary>
public class AlbumBox : MonoBehaviour
{
    [Header("Album")]
    public GameObject album;
    public Transform albumDropTarget;
    public float dropDuration = 0.5f;

    [Header("Girl Final Position")]
    public Transform girlFinalPosition;

    private bool albumDropped = false;

    public void DropAlbum()
    {
        if (albumDropped || album == null) return;
        albumDropped = true;
        StartCoroutine(DropRoutine());
    }

    IEnumerator DropRoutine()
    {
        Vector3 start = album.transform.position;
        Vector3 end = albumDropTarget != null
            ? albumDropTarget.position
            : start + Vector3.down * 1.5f;

        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.15f; // 轻微弧线
            album.transform.position = pos;
            yield return null;
        }
        album.transform.position = end;
        Debug.Log("[AlbumBox] Album dropped!");

        var girl = Level2Manager.Instance?.littleGirl;
        if (girl != null && girlFinalPosition != null)
        {
            // 确保关闭跟随猫模式，让她专心走向相册
            girl.followCatMode = false;

            girl.StartMovingTo(girlFinalPosition, onArrival: () =>
            {
                // 【核心修改】：到达目的地后，调用刚才写好的 PlayPickUp 方法
                girl.PlayPickUp();

                Debug.Log("[Girl] Reached the album and playing pick up animation!");

                // 延迟 2.5 秒结束关卡，留出时间让玩家看完弯腰捡东西的动画
                Level2Manager.Instance?.Invoke("OnLevelComplete", 2.5f);
            });
        }
    }
}