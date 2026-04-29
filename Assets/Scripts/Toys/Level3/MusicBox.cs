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

            // zoom out 修复: 玩家操作完成瞬间 (按下 Space) 立刻退出附身
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.isPossessing && player.currentToy == this)
            {
                player.ExitPossess();
                Debug.Log("[MusicBox] Auto-exited possession.");
            }

            // 启动协程继续处理 BGM 恢复 + 通知 Manager
            StartCoroutine(MusicSequence(bgm));
        }
    }

    System.Collections.IEnumerator MusicSequence(AudioSource bgm)
    {
        float musicLength = 3f;
        if (audioSource != null && audioSource.clip != null)
            musicLength = audioSource.clip.length;

        yield return new WaitForSeconds(musicLength);

        // 恢复 BGM
        if (bgm != null)
        {
            bgm.UnPause();
            Debug.Log("[MusicBox] BGM resumed.");
        }

        yield return new WaitForSeconds(catDelayAfterMusic);
        Level3Manager.Instance?.OnMusicPlayed();
        // 注: ExitPossess 已经在按下 Space 时执行了, 这里不需要再调
    }
    
    public override void Possess()
    {
        base.Possess();
        Debug.Log("[MusicBox] Possessed - Press Space to play music!");
    }
}