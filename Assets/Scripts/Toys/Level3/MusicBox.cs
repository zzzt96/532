using UnityEngine;

/// <summary>
/// 八音盒
/// 附身后按Space → 播放音乐 → 小猫被吸引跳上桌面
/// </summary>
public class MusicBox : ToyBase
{
    [Header("Audio")]
    public AudioSource audioSource;

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

            if (audioSource != null)
                audioSource.Play();

            Debug.Log("[MusicBox] Music playing! Cat attracted.");
            Level3Manager.Instance?.OnMusicPlayed();
        }
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[MusicBox] Possessed - Press Space to play music!");
    }
}