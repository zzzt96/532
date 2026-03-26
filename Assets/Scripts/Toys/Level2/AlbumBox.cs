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
            // 关闭跟随模式，走到指定位置坐下
            girl.followCatMode = false;
            girl.StartMovingTo(girlFinalPosition, onArrival: () =>
            {
                girl.SitDown();
                Debug.Log("[Girl] Picked up album. Level complete!");
                Level2Manager.Instance?.OnLevelComplete();
            });
        }
        else
        {
            Level2Manager.Instance?.OnLevelComplete();
        }
    }
}