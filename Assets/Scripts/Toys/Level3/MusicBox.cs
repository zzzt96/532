using UnityEngine;

public class MusicBox : ToyBase
{
    public AudioSource audioSource;
    public float catDelayAfterMusic = 3f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("注: 八音盒的音乐 wav 直接拖到上面的 audioSource.clip 字段, 不用 SoundSlot")]
    public bool musicBoxNote;  // 仅作为 Inspector 注释占位
    // ===============================================

    bool activated = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
    }

    public override void ToyUpdate()
    {
        if (activated) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            activated = true;

            // 暂停 BGM
            var bgmObj = GameObject.Find("BGM");
            AudioSource bgm = null;
            if (bgmObj != null) bgm = bgmObj.GetComponent<AudioSource>();
            if (bgm != null) bgm.Pause();

            if (audioSource != null) audioSource.Play();
            Debug.Log("[MusicBox] Music playing! BGM paused.");

            // 等八音盒音乐播完, 恢复 BGM, 再通知 Manager
            StartCoroutine(MusicSequence(bgm));
        }
    }

    System.Collections.IEnumerator MusicSequence(AudioSource bgm)
    {
        // 等八音盒音乐播完
        float musicLength = 3f;  // 默认 3 秒, 防止 audioSource.clip 为空导致死循环
        if (audioSource != null && audioSource.clip != null)
            musicLength = audioSource.clip.length;

        yield return new WaitForSeconds(musicLength);

        // 恢复 BGM (八音盒结束 → BGM 接回来)
        if (bgm != null)
        {
            bgm.UnPause();
            Debug.Log("[MusicBox] BGM resumed.");
        }

        // 再延迟 catDelayAfterMusic 秒后通知 Manager 触发猫
        yield return new WaitForSeconds(catDelayAfterMusic);
        Level3Manager.Instance?.OnMusicPlayed();

        // ★ zoom out 修复: 整个音乐流程结束, 玩家退出附身
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[MusicBox] Auto-exited possession.");
        }
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[MusicBox] Possessed - Press Space to play music!");
    }
}