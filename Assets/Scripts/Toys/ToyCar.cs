using UnityEngine;
using System.Collections;

public class ToyCar : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float minX = -35f;
    public float maxX = 5f;

    [Header("State")]
    public bool isActivated = false;    // 水杯触发后变 true

    [Header("Ground")]
    public float boardY = -100f;        // 自动记录木板高度
    public float groundY = 0.5f;
    public float boardMinX = -22f;
    public float boardMaxX = -14f;
    public float fallSpeed = 10f;

    [Header("Shelf Detection")]
    public WoodenShelf targetShelf;
    public float shelfHitDistance = 2.0f;

    [Header("Audio")]
    public AudioClip hitShelfSound; // 撞击架子音效

    private float lockedZ;
    private float originalZ;
    private bool hasHitShelf = false;
    private float debugTimer = 0f;
    public float possessZOffset = 1.5f;

    protected override void Start()
    {
        base.Start(); // 确保调用父类 Start 获取 AudioSource
        canBePossessed = false;
        lockedZ = transform.position.z;
        boardY = transform.position.y;  // 记录木板上的初始高度

        // 小车在木板上不需要物理，用 Transform 控制
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    /// <summary>
    /// 水杯砸到木板后调用，小车震动一下 + 变为可附身
    /// </summary>
    public void ActivateOnBoard()
    {
        if (isActivated) return;
        isActivated = true;
        canBePossessed = true;
        StartCoroutine(ShakeEffect());
        Debug.Log("[ToyCar] Activated on board! Ready to possess.");
    }

    System.Collections.IEnumerator ShakeEffect()
    {
        Vector3 originalPos = transform.position;
        float duration = 0.3f;
        float magnitude = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float y = originalPos.y + Random.Range(-magnitude, magnitude);
            transform.position = new Vector3(x, y, originalPos.z);
            yield return null;
        }

        transform.position = originalPos;
    }

    // 封装一个独立的撞倒架子的方法，避免多处代码重复
    void DoHitShelf(WoodenShelf shelf)
    {
        if (shelf != null && !hasHitShelf)
        {
            hasHitShelf = true;
            shelf.KnockDown();

            // 播放撞击音效
            if (audioSrc && hitShelfSound) audioSrc.PlayOneShot(hitShelfSound);

            Debug.Log("[ToyCar] HIT SHELF!");
        }
    }

    void Update()
    {
        if (!isActivated) return;

        if (!hasHitShelf && targetShelf != null)
        {
            float xDist = Mathf.Abs(transform.position.x - targetShelf.transform.position.x);

            debugTimer += Time.deltaTime;
            if (debugTimer > 0.5f)
            {
                debugTimer = 0f;
                Debug.Log($"[ToyCar] Car X={transform.position.x:F1}, Shelf X={targetShelf.transform.position.x:F1}, dist={xDist:F2}");
            }

            if (xDist < shelfHitDistance)
            {
                DoHitShelf(targetShelf);
            }
        }
    }

    public override void Possess()
    {
        base.Possess();
        originalZ = lockedZ;
        lockedZ = originalZ + possessZOffset;
        Debug.Log($"[ToyCar] Possessed! Z: {originalZ} → {lockedZ}");
    }

    public override void UnPossess()
    {
        base.UnPossess();
        lockedZ = originalZ;
    }

    public override void ToyUpdate()
    {
        if (!isActivated) return;

        Vector3 pos = transform.position;
        pos.z = lockedZ;

        // 判断是否还在木板上
        bool onBoard = (pos.x >= boardMinX && pos.x <= boardMaxX);

        if (onBoard)
        {
            // 在木板上：保持木板高度
            pos.y = boardY;
        }
        else
        {
            // 离开木板：下落到地面
            if (pos.y > groundY + 0.05f)
            {
                pos.y = Mathf.MoveTowards(pos.y, groundY, fallSpeed * Time.deltaTime);
            }
            else
            {
                pos.y = groundY;
            }
        }

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 90, 0);

        // AD 控制
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move = 1f;
        if (Input.GetKey(KeyCode.D)) move = -1f;

        if (Mathf.Abs(move) > 0.01f)
        {
            Vector3 newPos = transform.position + Vector3.right * move * moveSpeed * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            transform.position = newPos;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        WoodenShelf shelf = collision.collider.GetComponent<WoodenShelf>();
        if (shelf == null) shelf = collision.collider.GetComponentInParent<WoodenShelf>();
        DoHitShelf(shelf);
    }

    void OnTriggerEnter(Collider other)
    {
        WoodenShelf shelf = other.GetComponent<WoodenShelf>();
        if (shelf == null) shelf = other.GetComponentInParent<WoodenShelf>();
        DoHitShelf(shelf);
    }
}