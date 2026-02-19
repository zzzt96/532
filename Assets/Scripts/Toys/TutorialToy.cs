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
        // ȷ��һ��ʼ�Ϳ��Ա�����
        canBePossessed = true;
    }

    public override void ToyUpdate()
    {
        // ������ WASD �ƶ�
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        // ��һ�����򣬷�ֹб���ƶ�����
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // �����ƶ���Χ
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        // ���� Y �ᣬ��ֹ�������������ȥ (�������ĳ�ʼ�߶Ⱦ�����ȷ��)
        pos.y = transform.position.y;

        transform.position = pos;
    }
}