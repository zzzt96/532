using UnityEngine;

public class TutorialToy : ToyBase
{
    [Header("Tutorial Movement")]
    public float moveSpeed = 3f;

    [Header("Movement Bounds")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    [Header("Hit Detection")]
    public TutorialBall targetBall;
    public float hitDistance = 0.8f;
    public Transform hitPoint; 

    private bool hasHit = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = true;
        useXOnlyDetection = true;
    }

    public override void ToyUpdate()
    {
        // 检测是否撞到球（距离判断，不靠物理）
        if (!hasHit && targetBall != null)
        {
            Vector3 checkPos = hitPoint != null ? hitPoint.position : transform.position;
            float dist = Vector2.Distance(
                new Vector2(checkPos.x, checkPos.z),
                new Vector2(targetBall.transform.position.x, targetBall.transform.position.z)
            );

            if (dist <= hitDistance)
            {
                hasHit = true;
                targetBall.OnHitByTrain();

                // 火车任务完成，标记不可再附身
                GetComponent<InteractableTag>()?.SetCompleted();
                Debug.Log("[TutorialToy] Hit ball!");
            }
        }

        // WASD 移动
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}