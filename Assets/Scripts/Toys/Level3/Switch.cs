using UnityEngine;
public class Switch : ToyBase
{
    [Header("Switch Animation")]
    [Tooltip("开关拨杆Transform（可选，有的话会做翻转动画）")]
    public Transform leverTransform;
    [Tooltip("拨杆初始旋转")]
    private Quaternion leverStartRot;
    [Tooltip("拨动后的旋转（绕Z轴转多少度）")]
    public float leverFlipAngle = 40f;

    bool activated = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false; 

        if (leverTransform != null)
            leverStartRot = leverTransform.localRotation;
    }

    public override void ToyUpdate()
    {
        if (activated) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Activate();
        }
    }

    void Activate()
    {
        activated = true;
        Debug.Log("[SwitchL3] Switch activated!");

        // 拨杆动画（如果有）
        if (leverTransform != null)
        {
            Quaternion flipped = Quaternion.Euler(leverFlipAngle, 0f, 0f) * leverStartRot;
            leverTransform.localRotation = flipped;
        }
        
        Level3Manager.Instance?.OnLightsOn();
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[SwitchL3] Possessed - Press Space to turn on lights");
    }
}