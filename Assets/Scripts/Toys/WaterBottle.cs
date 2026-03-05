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
    public float tableEdgeY = 1.2f;   // 桌面高度（在Inspector里对照实际桌面Y设置）
    public float landingY = 0.1f;     // 地面落点高度
    public float rollDuration = 0.4f;
    public float fallDuration = 0.3f;
    public float bounceDuration = 0.15f;

    [Header("References")]
    public CatNPC cat;                // 拖入 Cat_L_Black

    [Header("Audio")]
    public AudioClip dropGroundSound;

    // 记录掉落时的X位置，用于动态计算落点
    private float fallFromX;
    private float fallFromZ;

    public override void ToyUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        transform.position += new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        Vector3 pos = transform.position;

        // 超出桌面边界 → 触发掉落
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

        // 记录当前X和Z，作为整个掉落路径的基准
        fallFromX = transform.position.x;
        fallFromZ = transform.position.z;

        StartCoroutine(ScriptedFall());
    }

    IEnumerator ScriptedFall()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion tiltedRot = startRot * Quaternion.Euler(0, 0, -90f);

        // ========== 阶段1：在桌面倒下滚到边缘（保持当前X，Y锁定桌面高度）==========
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

        // ========== 阶段2：从桌边垂直落到地面（保持同一X）==========
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

        // 落地音效
        if (audioSrc && dropGroundSound)
            audioSrc.PlayOneShot(dropGroundSound);

        // ========== 阶段3：弹跳 ==========
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

        // ========== 触发猫咪 ==========
        if (cat != null)
        {
            cat.AttractedBySound(landPos);
            Debug.Log("[WaterBottle] Sound triggered cat!");
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // 保留旧的球碰撞触发接口
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
}