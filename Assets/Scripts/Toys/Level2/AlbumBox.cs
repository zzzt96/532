using UnityEngine;
using System.Collections;

/// <summary>
/// 木箱 + 相册 - 猫跳上来后相册掉落
/// 相册落地后女孩走过来捡起，触发关卡结束
/// </summary>
public class AlbumBox : MonoBehaviour
{
    [Header("Album")]
    [Tooltip("相册 GameObject")]
    public GameObject album;
    [Tooltip("相册掉落的目标位置（地面上的空物体）")]
    public Transform albumDropTarget;
    public float dropDuration = 0.6f;

    private bool albumDropped = false;

    /// <summary>由 Level2Manager.OnCatOnAlbumBox() 调用</summary>
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
            // 轻微弧线掉落
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.1f;
            album.transform.position = pos;
            yield return null;
        }
        album.transform.position = end;
        Debug.Log("[AlbumBox] Album dropped!");

        // 女孩走到相册位置坐下（由 Level2Manager 里的 onArrival 回调处理）
    }
}