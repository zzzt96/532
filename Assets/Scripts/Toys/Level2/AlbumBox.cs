using UnityEngine;
using System.Collections;

public class AlbumBox : MonoBehaviour
{
    [Header("Album")]
    public GameObject album;
    public Transform albumDropTarget;
    public float dropDuration = 0.5f;

    [Header("Girl Final Position")]
    public Transform girlFinalPosition;

    [Header("Level Transition")]
    [Tooltip("女孩到达相册后多少秒切换到 Level 3 (留一点时间让玩家看清画面)")]
    public float levelTransitionDelay = 1.0f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("相册落地声 (厚重'啪'声 + 内页轻微抖动)")]
    public SoundSlot albumLandSound;
    // ===============================================

    private bool albumDropped = false;
    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    public void DropAlbum()
    {
        if (albumDropped || album == null) return;
        albumDropped = true;

        // 强制玩家退出附身, zoom out 看完整结尾演出
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing)
        {
            player.ExitPossess();
            Debug.Log("[AlbumBox] Auto-exited possession for ending cinematic.");
        }

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
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.15f;
            album.transform.position = pos;
            yield return null;
        }
        album.transform.position = end;

        // 相册落地瞬间播放音效
        PlayOneShotSlot(albumLandSound);

        Debug.Log("[AlbumBox] Album dropped!");

        var girl = Level2Manager.Instance?.littleGirl;
        if (girl != null && girlFinalPosition != null)
        {
            girl.followCatMode = false;

            girl.StartMovingTo(girlFinalPosition, onArrival: () =>
            {
                Debug.Log("[Girl] Reached the album. Transitioning to Level 3...");

                // 简化版终点: 直接切换到 Level 3, 不播放捡起动画
                Level2Manager.Instance?.Invoke("OnLevelComplete", levelTransitionDelay);
            });
        }
    }

    void PlayOneShotSlot(SoundSlot slot)
    {
        if (slot == null || slot.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.pitch = slot.pitch +
            Random.Range(-slot.randomPitchRange, slot.randomPitchRange);
        audioSrc.PlayOneShot(slot.clip, slot.volume);
    }
}