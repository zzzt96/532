using UnityEngine;

public class WoodenShelf : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 80f;
    public float targetAngle = 35f;
    public bool fallLeft = true;

    [Header("References")]
    public GameObject toyRocket;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("木架开始倒下时的木质倒地声")]
    public SoundSlot fallSound;

    [Tooltip("木架倒在床架上的二次撞击声 (倒下动画结束瞬间播放)")]
    public SoundSlot hitBedFrameSound;
    // ===============================================

    [Header("State")]
    public bool hasKnockedDown = false;
    private bool isFalling = false;
    private float currentAngle = 0f;
    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    public void KnockDown()
    {
        if (hasKnockedDown) return;
        hasKnockedDown = true;
        isFalling = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 开始倒下瞬间: 播放木架倒塌声
        PlayOneShotSlot(fallSound);

        Debug.Log("[WoodenShelf] Knocked down! Starting to fall...");
    }

    void Update()
    {
        if (!isFalling) return;

        float step = fallSpeed * Time.deltaTime;
        currentAngle += step;

        float direction = fallLeft ? 1f : -1f;
        transform.Rotate(Vector3.forward, step * direction, Space.World);

        if (currentAngle >= targetAngle)
        {
            isFalling = false;

            // 倒下动画结束瞬间: 播放撞床架声
            PlayOneShotSlot(hitBedFrameSound);

            Debug.Log("[WoodenShelf] Fell down and hit bed frame!");
            ActivateRocket();
        }
    }

    void ActivateRocket()
    {
        if (toyRocket == null)
        {
            Debug.LogWarning("[WoodenShelf] toyRocket is null!");
            return;
        }

        ToyRocket rocket = toyRocket.GetComponent<ToyRocket>();
        if (rocket != null)
        {
            rocket.Activate();
        }

        Debug.Log("[WoodenShelf] Rocket activated!");
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