using UnityEngine;
using System.Collections;

public class WaterBottle : ToyBase
{
    [Header("State")]
    public bool isKnockedDown = false;

    [Header("Scripted Fall Path")]
    public Transform tableEdgePoint;    // ★ 空物体1：桌子边缘（桌面高度，桌子最左/右端）
    public Transform landingPoint;      // ★ 空物体2：木板上的落点（小车旁边）
    public float rollDuration = 0.4f;   // 滚到桌边的时间
    public float fallDuration = 0.3f;   // 掉落时间
    public float bounceDuration = 0.15f;// 落地弹跳时间

    [Header("References")]
    public CarDropBoard carDropBoard;

    [Header("Audio")]
    public AudioClip dropGroundSound; // 掉落到地面的音效

    public override void ToyUpdate() { }

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;

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
        StartCoroutine(ScriptedFall());
    }

    IEnumerator ScriptedFall()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion tiltedRot = startRot * Quaternion.Euler(0, 0, -90f);

        // ========== 阶段1：在桌面上滚到桌边 ==========
        Vector3 edgePos = (tableEdgePoint != null)
            ? tableEdgePoint.position
            : startPos + new Vector3(-2f, 0f, 0f);

        // 保持桌面高度（不穿模）
        edgePos.y = startPos.y;
        edgePos.z = startPos.z;

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

        // ========== 阶段2：从桌边掉到木板落点 ==========
        Vector3 fallStart = edgePos;
        Vector3 landPos = (landingPoint != null)
            ? landingPoint.position
            : new Vector3(edgePos.x, edgePos.y - 3f, edgePos.z);
        landPos.z = startPos.z; // 锁定 Z

        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float gravity = t * t; // 加速下落

            transform.position = Vector3.Lerp(fallStart, landPos, gravity);
            transform.rotation = tiltedRot * Quaternion.Euler(0, 0, t * 180f);
            yield return null;
        }
        transform.position = landPos;

        // --- ★ 掉落到地面的瞬间播放音效 ★ ---
        if (audioSrc && dropGroundSound) audioSrc.PlayOneShot(dropGroundSound);

        // ========== 阶段3：落地弹跳（碰撞感） ==========
        float bounceHeight = 0.3f;
        Vector3 bounceUp = landPos + Vector3.up * bounceHeight;

        // 弹起
        elapsed = 0f;
        float halfBounce = bounceDuration * 0.4f;
        while (elapsed < halfBounce)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfBounce;
            transform.position = Vector3.Lerp(landPos, bounceUp, t);
            yield return null;
        }
        // 落回
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

        // ========== 触发小车 ==========
        if (carDropBoard != null)
        {
            carDropBoard.TriggerDrop();
            Debug.Log("[WaterBottle] Fall complete. Car triggered!");
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
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
}