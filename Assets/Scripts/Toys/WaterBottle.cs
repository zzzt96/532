using UnityEngine;

public class WaterBottle : ToyBase
{
    [Header("State")]
    public bool isKnockedDown = false;

    [Header("Control (while possessed)")]
    public float moveAccel = 18f;
    public float maxXZSpeed = 3.5f;

    [Header("Knockdown")]
    public float knockImpulse = 2.5f;
    public float knockTorque = 2.0f;
    
    // 记录初始 Z，掉落时锁定
    private float lockedZ;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        base.Start();  
        canBePossessed = false;
        
        lockedZ = transform.position.z;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    public void KnockDown(Vector3 hitDirection)
    {
        if (isKnockedDown) return;

        isKnockedDown = true;
        canBePossessed = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // 冻结 Z 轴移动 + Z 轴旋转，水杯只能在 X-Y 平面运动
            rb.constraints = RigidbodyConstraints.FreezePositionZ
                           | RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationY;

            // 只给 X 方向的力，不给 Z 方向
            Vector3 dir = hitDirection;
            dir.z = 0f; // 清除 Z 方向分量
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;
            dir = dir.normalized;

            rb.AddForce(dir * knockImpulse, ForceMode.Impulse);
            rb.AddTorque(Vector3.forward * knockTorque, ForceMode.Impulse);
        }

        Debug.Log("[WaterBottle] Knocked down! Now can possess. Z locked at: " + lockedZ);
    }

    public override void ToyUpdate()
    {
        if (!isKnockedDown || rb == null) return;

        if (rb.isKinematic) rb.isKinematic = false;
        if (!rb.useGravity) rb.useGravity = true;

        // 保持 Z 轴冻结
        if ((rb.constraints & RigidbodyConstraints.FreezePositionZ) == 0)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ
                           | RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationY;
        }

        // AD 只控制 X 方向
        float x = 0f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;

        if (Mathf.Abs(x) > 0.01f)
        {
            rb.AddForce(Vector3.right * x * moveAccel, ForceMode.Acceleration);
        }

        // 限速（只限 X，Y 让重力自然处理）
        Vector3 v = rb.linearVelocity;
        if (Mathf.Abs(v.x) > maxXZSpeed)
        {
            rb.linearVelocity = new Vector3(Mathf.Sign(v.x) * maxXZSpeed, v.y, 0f);
        }

        // 强制锁定 Z 位置（双保险）
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.z - lockedZ) > 0.01f)
        {
            pos.z = lockedZ;
            transform.position = pos;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isKnockedDown && collision.collider.CompareTag("Ball"))
        {
            Vector3 dir = (transform.position - collision.transform.position);
            KnockDown(dir);
            return;
        }

        if (isKnockedDown)
        {
            CarDropBoard board = collision.collider.GetComponent<CarDropBoard>();
            if (board == null) board = collision.collider.GetComponentInParent<CarDropBoard>();

            if (board != null)
            {
                board.TriggerDrop();
                Debug.Log("[WaterBottle] Hit CarDropBoard!");
            }
        }
    }
}