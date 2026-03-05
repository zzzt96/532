using UnityEngine;
using System.Collections;

public class Ball : ToyBase
{
    [Header("Movement on Table")]
    public float moveSpeed = 3f;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpHeight = 6f;
    public float jumpSpeed = 5f;

    [Header("Table Bounds")]
    public float minX = -35f;
    public float maxX = -25f;
    public float minZ = -8f;
    public float maxZ = -4f;
    public float fixedY = 4.59f;

    [Header("Knock Off Shelf")]
    public Transform landingOnDeskTarget;  // 空物体：左边桌面上的落点
    public float fallDuration = 0.4f;
    public float bounceDuration = 0.15f;
    private bool isKnockedOff = false;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip hitSound;
    public AudioClip dropSound;

    private bool isJumping = false;
    private float startY;
    private float jumpProgress = 0f;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false; // 默认不可附身，掉落后才可以
    }

    public override void ToyUpdate()
    {
        if (Input.GetKeyDown(jumpKey) && !isJumping)
            StartJump();

        if (isJumping)
        {
            UpdateJump();
            return;
        }

        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = fixedY;
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    void StartJump()
    {
        isJumping = true;
        startY = transform.position.y;
        jumpProgress = 0f;
        if (audioSrc && jumpSound) audioSrc.PlayOneShot(jumpSound);
    }

    void UpdateJump()
    {
        jumpProgress += jumpSpeed * Time.deltaTime;
        float height = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;

        Vector3 pos = transform.position;
        pos.y = startY + height;
        transform.position = pos;

        if (jumpProgress >= 1f)
        {
            isJumping = false;
            pos.y = fixedY;
            transform.position = pos;
        }
    }

    // 猫撞击后调用
    public void KnockOffShelf()
    {
        if (isKnockedOff) return;
        isKnockedOff = true;
        canBePossessed = false;
        StartCoroutine(FallToDesk());
    }

    IEnumerator FallToDesk()
    {
        Vector3 startPos = transform.position;

        // 落点：用 landingOnDeskTarget 的 XY，保持自身 Z
        Vector3 landPos = landingOnDeskTarget != null
            ? new Vector3(landingOnDeskTarget.position.x, landingOnDeskTarget.position.y, transform.position.z)
            : new Vector3(startPos.x, 0f, transform.position.z);

        // 掉落
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            transform.position = Vector3.Lerp(startPos, landPos, t * t); // 加速落下
            yield return null;
        }
        transform.position = landPos;

        // 落地音效
        if (audioSrc && dropSound) audioSrc.PlayOneShot(dropSound);

        // 弹跳
        Vector3 bounceUp = landPos + Vector3.up * 0.4f;
        elapsed = 0f;
        float half = bounceDuration * 0.4f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(landPos, bounceUp, elapsed / half);
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

        // 落稳后玩家可以附身
        canBePossessed = true;
        Debug.Log("[Ball] Landed on desk, ready to possess!");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Ball] Triggered: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("WaterBottle"))
        {
            WaterBottle bottle = other.GetComponent<WaterBottle>();
            if (bottle != null)
            {
                if (audioSrc && hitSound) audioSrc.PlayOneShot(hitSound);
                Vector3 dir = (other.transform.position - transform.position).normalized;
                if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;
                bottle.KnockDown(dir);
                Debug.Log("[Ball] Hit water bottle!");
            }
        }
        else if (other.CompareTag("IronHanger"))
        {
            if (audioSrc && hitSound) audioSrc.PlayOneShot(hitSound);
            other.gameObject.SendMessage("ActivateBunny", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("ActivateBunnyInternal", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
            Debug.Log("[Ball] Hit iron hanger!");
        }
    }
}