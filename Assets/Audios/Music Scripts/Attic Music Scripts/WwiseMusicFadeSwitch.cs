using UnityEngine;
using System.Collections;
using WwiseEvent = AK.Wwise.Event;

public class WwiseMusicFadeSwitch : MonoBehaviour
{
    [Header("Wwise Events")]
    public WwiseEvent firstEvent;   // 第一个音乐
    public WwiseEvent secondEvent;  // 第二个音乐

    [Header("Timing")]
    public float delayBeforeSwitch = 5f; // 播多久后切换
    public int fadeOutTimeMs = 1000;     // fade时间（毫秒）

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 1️⃣ 播放第一个音乐
        if (firstEvent != null && firstEvent.IsValid())
        {
            firstEvent.Post(gameObject);
        }

        // 2️⃣ 等待
        yield return new WaitForSeconds(delayBeforeSwitch);

        // 3️⃣ Fade Out 第一个音乐
        if (firstEvent != null)
        {
            AkSoundEngine.ExecuteActionOnEvent(
                firstEvent.Id,
                AkActionOnEventType.AkActionOnEventType_Stop,
                gameObject,
                fadeOutTimeMs
            );
        }

        // 👉 等 fade 完
        yield return new WaitForSeconds(fadeOutTimeMs / 1000f);

        // 4️⃣ 播放第二个音乐
        if (secondEvent != null && secondEvent.IsValid())
        {
            secondEvent.Post(gameObject);
        }
    }
}