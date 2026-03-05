using UnityEngine;
using System.Collections;

public class ToyCar : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float minX = -35f;
    public float maxX = 5f;

    [Header("State")]
    public bool isActivated = false;

    [Header("Ground")]
    public float boardY = -100f;
    public float groundY = 0.5f;
    public float boardMinX = -22f;
    public float boardMaxX = -14f;
    public float fallSpeed = 10f;

    [Header("Slide Off Board")]
    public float slideDuration = 0.4f;  // 从木板滑到边缘的时间
    public float slideDistance = 2f;    // 滑动距离

    [Header("Shelf Detection")]
    public WoodenShelf targetShelf;
    public float shelfHitDistance = 2.0f;

    [Header("Audio")]
    public AudioClip hitShelfSound;
    public AudioClip landSound;

    private float lockedZ;
    private float originalZ;
    private bool hasHitShelf = false;
    private float debugTimer = 0f;
    public float possessZOffset = 1.5f;
    private bool isSliding = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        lockedZ = transform.position.z;
        boardY = transform.position.y;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // 旧接口保留
    public void ActivateOnBoard()
    {
        if (isActivated) return;
        isActivated = true;
        canBePossessed = true;
        StartCoroutine(ShakeEffect());
        Debug.Log("[ToyCar] Activated on board!");
    }

    // 新接口：从木板滑落
    public void SlideOffBoard()
    {
        if (isActivated) return;
        isActivated = true;
        canBePossessed = false; // 落地后才能附身
        StartCoroutine(SlideAndFall());
    }

    IEnumerator SlideAndFall()
    {
        isSliding = true;
        Vector3 startPos = transform.position;
        // 向左滑出木板边缘，朝镜头方向（+Z）：
        Vector3 slideEnd = startPos + new Vector3(0, 0, slideDistance);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            transform.position = Vector3.Lerp(startPos, slideEnd, t * t);
            yield return null;
        }
        transform.position = slideEnd;

        // 滑出后开始自由落体到地面
        while (transform.position.y > groundY + 0.05f)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, fallSpeed * Time.deltaTime);
            transform.position = pos;
            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.y = groundY;
        transform.position = finalPos;

        isSliding = false;

        // 落地音效
        if (audioSrc && landSound) audioSrc.PlayOneShot(landSound);
        
        // 落地后更新 lockedZ 为当前实际位置
        lockedZ = transform.position.z;

        // 滑落后让车不再被视为在木板上
        boardMinX = 0f;
        boardMaxX = 0f;  // 清空范围，X永远不会在这个范围内

        canBePossessed = true;
        StartCoroutine(ShakeEffect());
    }

    IEnumerator ShakeEffect()
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

    void DoHitShelf(WoodenShelf shelf)
    {
        if (shelf != null && !hasHitShelf)
        {
            hasHitShelf = true;
            shelf.KnockDown();
            if (audioSrc && hitShelfSound) audioSrc.PlayOneShot(hitShelfSound);
            Debug.Log("[ToyCar] HIT SHELF!");
        }
    }

    void Update()
    {
        if (!isActivated || isSliding) return;

        if (!hasHitShelf && targetShelf != null)
        {
            float xDist = Mathf.Abs(transform.position.x - targetShelf.transform.position.x);
            debugTimer += Time.deltaTime;
            if (debugTimer > 0.5f)
            {
                debugTimer = 0f;
                Debug.Log($"[ToyCar] Car X={transform.position.x:F1}, Shelf X={targetShelf.transform.position.x:F1}, dist={xDist:F2}");
            }
            if (xDist < shelfHitDistance) DoHitShelf(targetShelf);
        }
    }

    public override void Possess()
    {
        base.Possess();
        originalZ = lockedZ;
        lockedZ = originalZ + possessZOffset;
    }

    public override void UnPossess()
    {
        base.UnPossess();
        lockedZ = originalZ;
    }

    public override void ToyUpdate()
    {
        if (!isActivated || isSliding) return;

        Vector3 pos = transform.position;
        pos.z = lockedZ;

        bool onBoard = (pos.x >= boardMinX && pos.x <= boardMaxX);
        if (onBoard)
            pos.y = boardY;
        else
        {
            if (pos.y > groundY + 0.05f)
                pos.y = Mathf.MoveTowards(pos.y, groundY, fallSpeed * Time.deltaTime);
            else
                pos.y = groundY;
        }

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, 90, 0);

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