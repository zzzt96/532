using System.Collections;
using UnityEngine;

public class BalloonL3 : MonoBehaviour
{
    [Header("Balloon Objects")]
    public GameObject balloonSmall;
    public GameObject balloonLarge;

    [Header("Books")]
    public Transform books;
    public float booksLiftY = 0.5f;
    public Vector3 booksFinalWorldPosition;
    public Vector3 booksFinalWorldRotation;
    public float booksLiftDuration = 0.2f;
    public float booksFallDuration = 0.6f;

    [Header("Desk Lamp")]
    public Light deskLampLight;
    public float lampIntensity = 3f;

    [Header("Balloon Inflation Animation")]
    public Vector3 balloonStartScale = new Vector3(0.3f, 0.3f, 0.3f);
    public Vector3 balloonEndScale = new Vector3(1f, 1f, 1f);

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("气球膨胀的橡胶拉伸声 (充气过程触发一次)")]
    public SoundSlot balloonStretchSound;
    // ===============================================

    AudioSource audioSrc;

    public void UpdateInflationProgress(float progress)
    {
        if (balloonSmall != null)
            balloonSmall.transform.localScale = Vector3.Lerp(balloonStartScale, balloonEndScale, progress);
    }

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        if (balloonSmall != null) balloonSmall.SetActive(true);
        if (balloonLarge != null) balloonLarge.SetActive(false);
        if (deskLampLight != null) deskLampLight.intensity = 0f;
    }

    public void TriggerInflate()
    {
        StartCoroutine(InflateSequence());
    }

    IEnumerator InflateSequence()
    {
        if (balloonSmall != null) balloonSmall.SetActive(false);
        if (balloonLarge != null) balloonLarge.SetActive(true);

        // 气球膨胀完成瞬间播橡胶拉伸声
        PlayOneShotSlot(balloonStretchSound);

        Debug.Log("[BalloonL3] Balloon inflated!");

        yield return new WaitForSeconds(0.3f);

        if (books != null)
            yield return StartCoroutine(LiftBooks());

        yield return new WaitForSeconds(0.2f);

        if (deskLampLight != null)
        {
            deskLampLight.intensity = lampIntensity;
            Debug.Log("[BalloonL3] Desk lamp on!");
        }
        Level3Manager.Instance?.OnDeskLampOn();
    }

    IEnumerator LiftBooks()
    {
        Vector3 startPos = books.position;
        Quaternion startRot = books.rotation;
        Vector3 liftPos = startPos + Vector3.up * booksLiftY;

        float elapsed = 0f;
        while (elapsed < booksLiftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / booksLiftDuration);
            books.position = Vector3.Lerp(startPos, liftPos, t);
            yield return null;
        }

        Quaternion endRot = Quaternion.Euler(booksFinalWorldRotation);
        Vector3 endPos = booksFinalWorldPosition;

        elapsed = 0f;
        while (elapsed < booksFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / booksFallDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            books.position = Vector3.Lerp(liftPos, endPos, eased);
            books.rotation = Quaternion.Slerp(startRot, endRot, eased);
            yield return null;
        }
        books.position = endPos;
        books.rotation = endRot;
        Debug.Log("[BalloonL3] Books fell to ground!");
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