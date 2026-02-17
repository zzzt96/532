using UnityEngine;

public class ToyCar : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("State")]
    public bool hasDropped = false;
    public bool hasLanded = false;

    [Header("Ground")]
    public float landingY = 1.0f;

    [Header("Shelf Detection")]
    public WoodenShelf targetShelf;
    public float shelfHitDistance = 2.0f;
    
    private float lockedZ;
    private float dropTimer = 0f;
    private bool hasHitShelf = false;
    private float debugTimer = 0f;

    protected override void Start()
    {
        base.Start();  
        canBePossessed = false;
        lockedZ = transform.position.z;
        rb = GetComponent<Rigidbody>();
    }

    public void Drop()
    {
        if (hasDropped) return;
        hasDropped = true;
        dropTimer = 0f;
        rb = GetComponent<Rigidbody>();
        Debug.Log("[ToyCar] Dropped from board!");
    }

    void Update()
    {
        // 掉落阶段：等落地
        if (hasDropped && !hasLanded)
        {
            dropTimer += Time.deltaTime;

            if (dropTimer > 0.5f && rb != null)
            {
                if (transform.position.y <= landingY && rb.linearVelocity.magnitude < 0.5f)
                {
                    OnLanded();
                }
            }

            if (dropTimer > 3f && !hasLanded)
            {
                OnLanded();
            }
        }

        // 架子检测放在 Update 里，不管是否附身都会检测
        if (hasLanded && !hasHitShelf)
        {
            if (targetShelf == null)
            {
                debugTimer += Time.deltaTime;
                if (debugTimer > 2f)
                {
                    debugTimer = 0f;
                    Debug.LogWarning("[ToyCar] TARGET SHELF IS NULL! Drag WoodenShelf into Inspector!");
                }
            }
            else
            {
                float xDist = Mathf.Abs(transform.position.x - targetShelf.transform.position.x);

                debugTimer += Time.deltaTime;
                if (debugTimer > 0.5f)
                {
                    debugTimer = 0f;
                    Debug.Log($"[ToyCar] Car X={transform.position.x:F1}, Shelf X={targetShelf.transform.position.x:F1}, dist={xDist:F2}, need<{shelfHitDistance}");
                }

                if (xDist < shelfHitDistance)
                {
                    hasHitShelf = true;
                    targetShelf.KnockDown();
                    Debug.Log($"[ToyCar] HIT SHELF! dist={xDist:F2}");
                }
            }
        }
    }

    void OnLanded()
    {
        if (hasLanded) return;
        hasLanded = true;
        canBePossessed = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log($"[ToyCar] Landed at Y={transform.position.y}! Now can possess.");
    }

    public override void ToyUpdate()
    {
        if (!hasLanded) return;

        // 强制 kinematic
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 锁定 Z
        Vector3 pos = transform.position;
        pos.z = lockedZ;
        transform.position = pos;

        // AD
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move = 1f;
        if (Input.GetKey(KeyCode.D)) move = -1f;

        if (Mathf.Abs(move) > 0.01f)
        {
            transform.position += Vector3.right * move * moveSpeed * Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        WoodenShelf shelf = collision.collider.GetComponent<WoodenShelf>();
        if (shelf == null) shelf = collision.collider.GetComponentInParent<WoodenShelf>();
        if (shelf != null && !hasHitShelf)
        {
            hasHitShelf = true;
            shelf.KnockDown();
            Debug.Log("[ToyCar] Hit WoodenShelf (collision)!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        WoodenShelf shelf = other.GetComponent<WoodenShelf>();
        if (shelf == null) shelf = other.GetComponentInParent<WoodenShelf>();
        if (shelf != null && !hasHitShelf)
        {
            hasHitShelf = true;
            shelf.KnockDown();
            Debug.Log("[ToyCar] Hit WoodenShelf (trigger)!");
        }
    }
}