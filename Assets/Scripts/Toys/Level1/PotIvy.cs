using UnityEngine;
using System.Collections;

public class PotIvy : MonoBehaviour
{
    [Header("References")]
    public CatNPC cat;
    public Transform boardPosition;

    [Header("Shake Settings")]
    public float shakeDuration = 1.2f;
    public float shakeMagnitude = 0.15f;
    public float shakeSpeed = 20f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("叶子沙沙/哗哗声 (盆栽被撞后摇晃发出的声音)")]
    public SoundSlot leavesShakeSound;
    // ===============================================

    private bool hasBeenHit = false;
    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit) return;
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            hasBeenHit = true;
            ball.SetPotLineDone();              // 只标记盆栽线完成, Ball 自己判断是否双线齐全
            ball.PlayHitPlantSoundExternal();   // 让 Ball 播自己的撞击声
            StartCoroutine(ShakeAndAttractCat());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;
        Ball ball = collision.collider.GetComponent<Ball>();
        if (ball != null)
        {
            hasBeenHit = true;
            ball.SetPotLineDone();
            ball.PlayHitPlantSoundExternal();
            StartCoroutine(ShakeAndAttractCat());
        }
    }

    IEnumerator ShakeAndAttractCat()
    {
        // 播放盆栽自己的叶子沙沙声
        PlayLeavesSound();

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;
            float fade = 1f - t;

            float offsetX = Mathf.Sin(elapsed * shakeSpeed) * shakeMagnitude * fade;
            float offsetZ = Mathf.Cos(elapsed * shakeSpeed * 0.7f) * shakeMagnitude * 0.5f * fade;

            transform.position = originalPos + new Vector3(offsetX, 0, offsetZ);
            transform.rotation = originalRot * Quaternion.Euler(0, 0, offsetX * 10f);
            yield return null;
        }

        transform.position = originalPos;
        transform.rotation = originalRot;

        if (cat != null && boardPosition != null)
        {
            cat.AttractedByIvy(boardPosition.position);
            Debug.Log("[PotIvy] Shook! Cat attracted to board.");
        }
    }
    
    void PlayLeavesSound()
    {
        if (leavesShakeSound == null || leavesShakeSound.clip == null) return;
        if (audioSrc == null) return;

        audioSrc.pitch = leavesShakeSound.pitch +
            Random.Range(-leavesShakeSound.randomPitchRange, leavesShakeSound.randomPitchRange);
        audioSrc.PlayOneShot(leavesShakeSound.clip, leavesShakeSound.volume);
    }
}