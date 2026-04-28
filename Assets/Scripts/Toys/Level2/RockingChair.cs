using UnityEngine;
using System.Collections;

public class RockingChair : MonoBehaviour
{
    [Header("Rocking")]
    public float rockAngle = 4f;
    public float rockSpeed = 1f;
    public float rockDuration = 3f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("摇椅节奏性吱呀吱呀声 (建议勾选 Loop, 摇动期间持续播)")]
    public SoundSlot rockingSound;
    // ===============================================

    private bool isRocking = false;
    private Quaternion initialRotation;
    private AudioSource audioSrc;

    void Start()
    {
        initialRotation = transform.localRotation;
        audioSrc = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (!isRocking) StartRocking();
    }

    public void StartRocking()
    {
        if (isRocking) return;
        isRocking = true;
        StartCoroutine(RockRoutine());
    }

    IEnumerator RockRoutine()
    {
        // 启动 loop 摇椅吱呀声
        PlayLoopSlot(rockingSound);

        float elapsed = 0f;
        while (elapsed < rockDuration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * rockSpeed * Mathf.PI) * rockAngle;
            transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = initialRotation;
        isRocking = false;

        // 停止摇椅声
        StopLoopSound();

        Debug.Log("[RockingChair] Attracted the cat!");
        Level2Manager.Instance?.cat?.GoToRockingChair();
    }

    void PlayLoopSlot(SoundSlot slot)
    {
        if (slot == null || slot.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.clip = slot.clip;
        audioSrc.volume = slot.volume;
        audioSrc.pitch = slot.pitch +
            Random.Range(-slot.randomPitchRange, slot.randomPitchRange);
        audioSrc.loop = true;
        audioSrc.Play();
    }

    void StopLoopSound()
    {
        if (audioSrc == null) return;
        if (audioSrc.isPlaying)
        {
            audioSrc.Stop();
            audioSrc.loop = false;
        }
    }
}