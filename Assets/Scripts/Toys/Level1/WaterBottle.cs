using UnityEngine;
using System.Collections;

public class WaterBottle : ToyBase
{
    [Header("State")]
    public bool isKnockedDown = false;

    [Header("WASD Movement on Table")]
    public float moveSpeed = 2f;
    public float minX = -1f;
    public float maxX = 1f;
    public float minZ = -0.5f;
    public float maxZ = 0.5f;

    [Header("Scripted Fall Heights")]
    public float tableEdgeY = 1.2f;
    public float landingY = 0.1f;
    public float rollDuration = 0.4f;
    public float fallDuration = 0.3f;
    public float bounceDuration = 0.15f;

    [Header("References")]
    public CatNPC cat;

    // ==================== Audio ===================
    [Header("Audio")]
    [Tooltip("移动摩擦声 (建议勾选 Loop, 提供可循环的短 wav)")]
    public SoundSlot moveSound;

    [Tooltip("金属落地声 (单次触发, 不需要 loop)")]
    public SoundSlot landSound;
    // ====================================================================

    private float fallFromX;
    private float fallFromZ;

    // 用于检测移动状态变化
    private bool wasMoving = false;

    public override void ToyUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        bool isMoving = (moveX != 0f || moveZ != 0f);

        // ===== 移动音效控制 =====
        if (isMoving && !wasMoving)
        {
            // 从静止 → 开始移动：启动循环摩擦声
            PlaySound(moveSound);
        }
        else if (!isMoving && wasMoving)
        {
            // 从移动 → 停止：停止循环音
            StopSound();
        }
        wasMoving = isMoving;

        transform.position += new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        Vector3 pos = transform.position;

        if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null) player.ExitPossess();

            KnockDown(Vector3.zero);
        }
    }

    protected override void Start()
    {
        base.Start();
        canBePossessed = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void KnockDown(Vector3 hitDirection)
    {
        if (isKnockedDown) return;
        isKnockedDown = true;
        canBePossessed = false;

        // 翻倒时确保停止移动循环音
        StopSound();

        fallFromX = transform.position.x;
        fallFromZ = transform.position.z;

        StartCoroutine(ScriptedFall());
    }

    IEnumerator ScriptedFall()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion tiltedRot = startRot * Quaternion.Euler(0, 0, -90f);

        // 阶段1：滚到桌边
        Vector3 edgePos = new Vector3(fallFromX, tableEdgeY, fallFromZ);
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rollDuration;
            float smooth = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, edgePos, smooth);
            transform.rotation = Quaternion.Lerp(startRot, tiltedRot, smooth);
            yield return null;
        }
        transform.position = edgePos;
        transform.rotation = tiltedRot;

        // 阶段2：垂直下落
        Vector3 landPos = new Vector3(fallFromX, landingY, fallFromZ);
        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float gravity = t * t;
            transform.position = Vector3.Lerp(edgePos, landPos, gravity);
            transform.rotation = tiltedRot * Quaternion.Euler(0, 0, t * 180f);
            yield return null;
        }
        transform.position = landPos;

        // ===== 落地音效 =====
        PlaySound(landSound);

        // 阶段3：弹跳
        Vector3 bounceUp = landPos + Vector3.up * 0.3f;
        elapsed = 0f;
        float halfBounce = bounceDuration * 0.4f;
        while (elapsed < halfBounce)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(landPos, bounceUp, elapsed / halfBounce);
            yield return null;
        }
        elapsed = 0f;
        float secondHalf = bounceDuration * 0.6f;
        while (elapsed < secondHalf)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / secondHalf;
            transform.position = Vector3.Lerp(bounceUp, landPos, t * t);
            yield return null;
        }
        transform.position = landPos;

        // 触发猫咪
        if (cat != null)
        {
            cat.AttractedBySound(landPos);
            Debug.Log("[WaterBottle] Sound triggered cat!");
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        GetComponent<InteractableTag>()?.SetCompleted();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isKnockedDown && other.CompareTag("Ball"))
            KnockDown(transform.position - other.transform.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isKnockedDown && collision.collider.CompareTag("Ball"))
            KnockDown(transform.position - collision.transform.position);
    }
    
    public override void UnPossess()
    {
        base.UnPossess();
        StopSound();
    }
}