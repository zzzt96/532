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

    protected override void Start()
    {
        base.Start();
        // 确保一开始就可以被附身
        canBePossessed = true;
    }

    public override void ToyUpdate()
    {
        // 基础的 WASD 移动
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        // 归一化方向，防止斜向移动过快
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // 限制移动范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        // 锁定 Y 轴，防止它飞起来或掉下去 (假设它的初始高度就是正确的)
        pos.y = transform.position.y;

        transform.position = pos;
    }
}