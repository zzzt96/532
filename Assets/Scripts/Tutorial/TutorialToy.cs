using UnityEngine;

public class TutorialToy : ToyBase
{
    [Header("Tutorial Movement")]
    public float moveSpeed = 3f;
    public float jumpForce = 5f; // ��Ծ����

    [Header("Movement Bounds")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    private bool isJumping = false;
    private float startY;

    protected override void Start()
    {
        base.Start();
        canBePossessed = true;
        useXOnlyDetection = true;
        startY = transform.position.y;
    }

    public override void ToyUpdate()
    {
        // ==== ������Ծ ====
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            if (rb != null)
            {
                // ��������и��壬ʹ�ø�����Ծ
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        // ���û�и��壬�ü򵥵Ĵ���ģ����Ծ����
        if (rb == null && isJumping)
        {
            transform.position += Vector3.up * jumpForce * Time.deltaTime;
            if (transform.position.y > startY + 2f) jumpForce = -Mathf.Abs(jumpForce); // �ﵽ���������
            if (transform.position.y <= startY && jumpForce < 0)
            {
                Vector3 resetPos = transform.position;
                resetPos.y = startY;
                transform.position = resetPos;
                isJumping = false;
                jumpForce = Mathf.Abs(jumpForce); // �ָ���ʼ����
            }
        }
        else if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            isJumping = false; // ������ؼ��
        }

        // ==== ���� WASD �ƶ� ====
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