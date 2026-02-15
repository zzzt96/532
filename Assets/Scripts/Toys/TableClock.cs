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

    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isBouncing = false;
    private float bounceProgress = 0f;
    private int currentBounce = 0;
    private float startY;
    private float currentBounceHeight;

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
        // 延迟等待
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

        Debug.Log("[TableClock] ALARM! Bouncing!");
    }
}