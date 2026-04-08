using UnityEngine;
public class MusicBox : ToyBase
{
    public AudioSource audioSource;
    public float catDelayAfterMusic = 3f;

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

            // 暂停BGM
            var bgmObj = GameObject.Find("BGM");
            if (bgmObj != null)
            {
                var bgm = bgmObj.GetComponent<AudioSource>();
                if (bgm != null) bgm.Pause();
            }

            if (audioSource != null) audioSource.Play();
            Debug.Log("[MusicBox] Music playing! BGM paused. Cat will arrive soon.");
            StartCoroutine(NotifyCatDelayed());
        }
    }

    System.Collections.IEnumerator NotifyCatDelayed()
    {
        yield return new WaitForSeconds(catDelayAfterMusic);
        Level3Manager.Instance?.OnMusicPlayed();
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[MusicBox] Possessed - Press Space to play music!");
    }
}