using UnityEngine;

/// <summary>
/// 测试用交互物体
/// 附身后按 WASD 移动，Space 键触发动作
/// 用于验证交互系统是否正常工作
/// </summary>
public class TestToy : ToyBase
{
    [Header("Test Settings")]
    public float moveSpeed = 3f;
    public string actionMessage = "Test action triggered!";

    public override void ToyUpdate()
    {
        if (!isPossessed) return;

        // WASD 控制移动（测试用）
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
        }
        else
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        // Space 键触发动作（测试用）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerAction();
        }
    }

    void TriggerAction()
    {
        Debug.Log(actionMessage);
        
        // 示例：触发小女孩继续移动
        LittleGirlController girl = FindObjectOfType<LittleGirlController>();
        if (girl != null)
        {
            girl.UnlockMovement();
            Debug.Log("Unlocked girl movement!");
        }
    }
}