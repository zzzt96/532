using UnityEngine;

public class ToyRocket : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Movement Bounds (X range on shelf)")]
    public float minX = -45f;
    public float maxX = -38f;

    [Header("State")]
    public bool isActivated = false;

    [Header("Fall Animation")]
    public float fallSpeed = 120f;
    public float fallTargetAngle = 90f;
    public bool fallLeft = true;
    private bool isFalling = false;
    private float currentFallAngle = 0f;

    [Header("Clock Detection")]
    public TableClock targetClock;
    public float clockHitDistance = 2.0f;

    private float lockedZ;
    private float lockedY;
    private bool hasHitClock = false;
    private float debugTimer = 0f;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        lockedZ = transform.position.z;
        lockedY = transform.position.y;
        maxX = transform.position.x + 1f;
    }

    public void Activate()
    {
        if (isActivated) return;
        isActivated = true;
        isFalling = true;
        canBePossessed = false;
        currentFallAngle = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("[ToyRocket] Starting to fall over...");
    }

    void Update()
    {
        if (isFalling)
        {
            float step = fallSpeed * Time.deltaTime;
            currentFallAngle += step;

            float direction = fallLeft ? 1f : -1f;
            transform.Rotate(0f, 0f, step * direction, Space.World);

            if (currentFallAngle >= fallTargetAngle)
            {
                isFalling = false;
                canBePossessed = true;
                lockedY = transform.position.y;

                Debug.Log($"[ToyRocket] Fell over! Now can possess. (Won't trigger clock until possessed)");
            }
            return;
        }

        // 只在被附身时才检测闹钟距离
        if (isActivated && !hasHitClock && isPossessed)
        {
            if (targetClock == null)
            {
                debugTimer += Time.deltaTime;
                if (debugTimer > 1f)
                {
                    debugTimer = 0f;
                    TableClock found = FindFirstObjectByType<TableClock>();
                    if (found != null)
                    {
                        targetClock = found;
                        Debug.Log("[ToyRocket] Auto-found TableClock: " + found.gameObject.name);
                    }
                    else
                    {
                        Debug.LogWarning("[ToyRocket] TARGET CLOCK IS NULL!");
                    }
                }
                return;
            }

            float xDist = Mathf.Abs(transform.position.x - targetClock.transform.position.x);

            debugTimer += Time.deltaTime;
            if (debugTimer > 0.5f)
            {
                debugTimer = 0f;
                Debug.Log($"[ToyRocket] Rocket X={transform.position.x:F1}, Clock X={targetClock.transform.position.x:F1}, dist={xDist:F2}");
            }

            if (xDist < clockHitDistance)
            {
                hasHitClock = true;
                targetClock.TriggerAlarm();
                Debug.Log("[ToyRocket] HIT CLOCK! Alarm triggered!");
            }
        }
    }

    public override void ToyUpdate()
    {
        if (!isActivated || isFalling) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Vector3 pos = transform.position;
        pos.z = lockedZ;
        pos.y = lockedY;

        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move = 1f;
        if (Input.GetKey(KeyCode.D)) move = -1f;

        if (Mathf.Abs(move) > 0.01f)
        {
            pos.x += move * moveSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }
}