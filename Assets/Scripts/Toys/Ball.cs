using UnityEngine;

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

    private bool isJumping = false;
    private float startY;
    private float jumpProgress = 0f;

    protected override void Start()
    {
        base.Start();
        canBePossessed = true;
    }

    public override void ToyUpdate()
    {
        // 跳跃逻辑（优先级最高）
        if (Input.GetKeyDown(jumpKey) && !isJumping)
        {
            StartJump();
        }

        if (isJumping)
        {
            UpdateJump();
            return; // 跳跃时不能水平移动
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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Ball] Triggered: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("WaterBottle"))
        {
            WaterBottle bottle = other.GetComponent<WaterBottle>();
            if (bottle != null)
            {
                // 传入一个方向，满足 KnockDown(Vector3)
                Vector3 dir = (other.transform.position - transform.position).normalized;
                if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;

                bottle.KnockDown(dir);
                Debug.Log("[Ball] Hit water bottle!");
            }
        }
        else if (other.CompareTag("IronHanger"))
        {
            //  用 SendMessage 避免方法名不匹配导致编译失败
            // 你 IronHanger 里不管是 ActivateBunny / ActivateBunnyInternal / Activate 都能兼容（有哪个就调用哪个）
            other.gameObject.SendMessage("ActivateBunny", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("ActivateBunnyInternal", SendMessageOptions.DontRequireReceiver);
            other.gameObject.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);

            Debug.Log("[Ball] Hit iron hanger!");
        }
    }
}