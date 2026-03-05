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

    [Header("Audio")]
    public AudioClip shakeSound;

    private bool hasBeenHit = false;
    private AudioSource audioSrc;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit) return;
        // 【修改这里】只用 GetComponent 检查，安全无报错！
        if (other.GetComponent<Ball>() != null)
        {
            hasBeenHit = true;
            StartCoroutine(ShakeAndAttractCat());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;
        // 【修改这里】
        if (collision.collider.GetComponent<Ball>() != null)
        {
            hasBeenHit = true;
            StartCoroutine(ShakeAndAttractCat());
        }
    }

    IEnumerator ShakeAndAttractCat()
    {
        if (audioSrc && shakeSound) audioSrc.PlayOneShot(shakeSound);

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;
        float elapsed = 0f;

        // 晃动动画
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;
            float fade = 1f - t; // 越来越小

            float offsetX = Mathf.Sin(elapsed * shakeSpeed) * shakeMagnitude * fade;
            float offsetZ = Mathf.Cos(elapsed * shakeSpeed * 0.7f) * shakeMagnitude * 0.5f * fade;

            transform.position = originalPos + new Vector3(offsetX, 0, offsetZ);
            transform.rotation = originalRot * Quaternion.Euler(0, 0, offsetX * 10f);
            yield return null;
        }

        transform.position = originalPos;
        transform.rotation = originalRot;

        // 晃动结束后吸引小猫
        if (cat != null && boardPosition != null)
        {
            cat.AttractedByIvy(boardPosition.position);
            Debug.Log("[PotIvy] Shook! Cat attracted to board.");
        }
    }
}