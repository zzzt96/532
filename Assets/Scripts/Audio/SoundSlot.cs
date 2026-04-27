using UnityEngine;

[System.Serializable]
public class SoundSlot
{
    [Tooltip("拖入音频文件 (wav/mp3)")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("音量大小 (0 = 静音, 1 = 最大)")]
    public float volume = 1f;

    [Range(0.5f, 2f)]
    [Tooltip("音高 (1 = 原始音高, <1 变低, >1 变高)")]
    public float pitch = 1f;

    [Range(0f, 0.3f)]
    [Tooltip("随机音高范围 (0 = 每次相同, >0 = 每次播放音高微变, 让重复音效不显单调)")]
    public float randomPitchRange = 0f;

    [Tooltip("是否循环播放 (用于持续性音效，如风扇运转、移动摩擦声)")]
    public bool loop = false;
    
    public float GetPitch()
    {
        if (randomPitchRange <= 0f) return pitch;
        return pitch + Random.Range(-randomPitchRange, randomPitchRange);
    }
    
    public bool IsValid()
    {
        return clip != null;
    }
}