using UnityEngine;

public class TableClock : MonoBehaviour
{
    [Header("State")]
    public bool hasTriggered = false;

    [Header("Delay")]
    public float alarmDelay = 1f;

    [Header("Bounce Effect")]
    public float bounceHeight = 3f;
    public float bounceSpeed = 5f;
    public int bounceCount = 3;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("闹铃响声 (建议勾选 Loop, 提供可循环的短 wav)")]
    public SoundSlot alarmSound;
    // ===============================================

    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isBouncing = false;
    private float bounceProgress = 0f;
    private int currentBounce = 0;
    private float startY;
    private float currentBounceHeight;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void TriggerAlarm()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        isWaiting = true;
        waitTimer = 0f;

        Debug.Log("[TableClock] Hit! Waiting before alarm...");
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= alarmDelay)
            {
                isWaiting = false;
                StartBounce();
            }
            return;
        }

        if (!isBouncing) return;

        bounceProgress += bounceSpeed * Time.deltaTime;

        float height = Mathf.Sin(bounceProgress * Mathf.PI) * currentBounceHeight;

        Vector3 pos = transform.position;
        pos.y = startY + Mathf.Max(0f, height);
        transform.position = pos;

        if (bounceProgress >= 1f)
        {
            currentBounce++;
            bounceProgress = 0f;
            currentBounceHeight *= 0.5f;

            if (currentBounce >= bounceCount)
            {
                isBouncing = false;
                pos.y = startY;
                transform.position = pos;
                Debug.Log("[TableClock] Bounce finished! Waking up girl now.");

                // 停止闹铃循环音
                StopAlarm();

                // 强制玩家退出附身 + zoom out
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && player.isPossessing)
                {
                    player.ExitPossess();
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.WakeUpGirl();
                }
            }
        }
    }

    void StartBounce()
    {
        isBouncing = true;
        bounceProgress = 0f;
        currentBounce = 0;
        startY = transform.position.y;
        currentBounceHeight = bounceHeight;
        
        PlayAlarm();

        Debug.Log("[TableClock] ALARM! Bouncing!");
    }

    void PlayAlarm()
    {
        if (alarmSound == null || alarmSound.clip == null) return;
        if (audioSource == null) return;

        audioSource.clip = alarmSound.clip;
        audioSource.volume = alarmSound.volume;
        audioSource.pitch = alarmSound.pitch + Random.Range(-alarmSound.randomPitchRange, alarmSound.randomPitchRange);
        audioSource.loop = true;
        audioSource.Play();
    }

    void StopAlarm()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}