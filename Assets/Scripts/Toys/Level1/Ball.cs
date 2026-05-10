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
    private bool pendingExitPossess = false;  // 跳跃中撞到东西, 等跳完再退出附身
    
    [Header("Table Bounds")]
    public float minX = -35f;
    public float maxX = -25f;
    public float minZ = -8f;
    public float maxZ = -4f;
    public float fixedY = 4.59f;

    [Header("Knock Off Shelf")]
    public Transform landingOnDeskTarget;
    public float fallDuration = 0.4f;
    public float bounceDuration = 0.15f;
    private bool isKnockedOff = false;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("跳跃时的弹跳声 (自找音源, 非 Berklee 列表)")]
    public SoundSlot jumpSound;

    [Tooltip("被猫撞落到桌面的落桌声 (轻质物体撞木桌)")]
    public SoundSlot landSound;

    [Tooltip("撞到墙上铁钩子的清脆碰撞声")]
    public SoundSlot hitHangerSound;

    [Tooltip("撞到盆栽的轻撞声 (由 PotIvy 通过 PlayHitPlantSoundExternal 触发)")]
    public SoundSlot hitPlantSound;

    [Tooltip("撞到水壶的撞击声")]
    public SoundSlot hitBottleSound;
    // ===============================================

    private bool isJumping = false;
    private float startY;
    private float jumpProgress = 0f;

    private bool lineIronDone = false;
    private bool linePotDone = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false; // 默认不可附身, 掉落到桌面后才变 true
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

    // ===== 双线路完成追踪 =====
    public void SetIronLineDone()
    {
        lineIronDone = true;
        Debug.Log("[Ball] Iron hanger line completed.");
        
        if (isJumping)
            pendingExitPossess = true;
        else
            ExitPossessIfActive();
        
        CheckBothLinesDone();
    }

    public void SetPotLineDone()
    {
        linePotDone = true;
        Debug.Log($"[Ball] Plant pot line completed. isJumping={isJumping}");

        if (isJumping)
        {
            pendingExitPossess = true;
            // Debug.Log("[Ball] Set pendingExitPossess = true (will exit after jump ends)");
        }
        else
        {
            // Debug.Log("[Ball] Not jumping, calling ExitPossessIfActive immediately");
            ExitPossessIfActive();
        }

        CheckBothLinesDone();
    }

    void CheckBothLinesDone()
    {
        if (lineIronDone && linePotDone)
        {
            // 只有两条线都完成时, Ball 才整体完成 + 不可再附身
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            Debug.Log("[Ball] Both lines completed! Ball locked.");
        }
    }

    // ===== 由 PotIvy 外部调用, 触发 Ball 自己的撞盆栽声 =====
    public void PlayHitPlantSoundExternal()
    {
        PlaySound(hitPlantSound);
    }

    // ===== 跳跃 =====
    void StartJump()
    {
        isJumping = true;
        startY = transform.position.y;
        jumpProgress = 0f;
        PlaySound(jumpSound);
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

            Debug.Log($"[Ball] Jump ended. pendingExitPossess={pendingExitPossess}");

            if (pendingExitPossess)
            {
                pendingExitPossess = false;
                // Debug.Log("[Ball] Calling ExitPossessIfActive from UpdateJump end");
                ExitPossessIfActive();
            }
        }
    }

    // ===== 被猫撞下书架 =====
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

        Vector3 landPos = landingOnDeskTarget != null
            ? new Vector3(landingOnDeskTarget.position.x, landingOnDeskTarget.position.y, transform.position.z)
            : new Vector3(startPos.x, 0f, transform.position.z);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            transform.position = Vector3.Lerp(startPos, landPos, t * t);
            yield return null;
        }
        transform.position = landPos;

        // 落桌音效
        PlaySound(landSound);

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

        canBePossessed = true;
        Debug.Log("[Ball] Landed on desk, ready to possess!");
    }

    // ===== 与其他物体碰撞 =====
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Ball] Triggered: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("WaterBottle"))
        {
            WaterBottle bottle = other.GetComponent<WaterBottle>();
            if (bottle != null)
            {
                PlaySound(hitBottleSound);
                Vector3 dir = (other.transform.position - transform.position).normalized;
                if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;
                bottle.KnockDown(dir);
                Debug.Log("[Ball] Hit water bottle!");
            }
        }
        else if (other.CompareTag("IronHanger"))
        {
            PlaySound(hitHangerSound);
            other.gameObject.SendMessage("ActivateBunny", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("ActivateBunnyInternal", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
            Debug.Log("[Ball] Hit iron hanger!");
            SetIronLineDone();
            // 注意: 这里不调 SetCompleted, 让玩家有机会走另一条线
        }
    }
    
    void ExitPossessIfActive()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        Debug.Log($"[Ball] ExitPossessIfActive: player={player}, isPossessing={player?.isPossessing}, currentToy={player?.currentToy}, this={this}");
    
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            Debug.Log("[Ball] Calling player.ExitPossess()");
            player.ExitPossess();
        }
        else
        {
            Debug.Log("[Ball] Conditions not met, NOT exiting possession");
        }
    }
}