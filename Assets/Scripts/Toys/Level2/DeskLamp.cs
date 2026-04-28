using UnityEngine;

public class DeskLamp : ToyBase
{
    [Header("Rotation")]
    public float rotateSpeed = 60f;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Light")]
    [Tooltip("台灯的 SpotLight 子物体，附身时打开")]
    public Light lampSpotLight;

    [Header("Lamp Beam")]
    public GameObject lampBeam;

    [Header("Album Box Zone")]
    public float boxAngleMin = -65f;
    public float boxAngleMax = -25f;
    public float holdTimeRequired = 0.8f;

    private float currentAngle = 0f;
    private float holdTimer = 0f;
    private bool triggered = false;
    private Quaternion initialRotation;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.localRotation;
        canBePossessed = false;
    }

    public override void Possess()
    {
        base.Possess();
        if (lampSpotLight != null) lampSpotLight.enabled = true;
        if (lampBeam != null) lampBeam.SetActive(true);
    }

    public override void UnPossess()
    {
        base.UnPossess();

        // 退出附身时关闭灯光和光束
        if (lampSpotLight != null) lampSpotLight.enabled = false;
        if (lampBeam != null) lampBeam.SetActive(false);
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log($"[Lamp] current angle: {currentAngle}");

        // 只接受 WASD, 不接受方向键
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input =  1f;

        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.localRotation = initialRotation * Quaternion.Euler(0f, currentAngle, 0f);

        bool inZone = currentAngle >= boxAngleMin && currentAngle <= boxAngleMax;
        holdTimer = inZone ? holdTimer + Time.deltaTime : 0f;

        if (holdTimer >= holdTimeRequired)
        {
            triggered = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            Debug.Log("[Lamp] Light on box! Cat jumping.");

            // 触发猫跳到纸盒子
            Level2Manager.Instance?.cat?.GoToAlbumBox();

            // 关键修复: 触发关卡终点演出后, 强制玩家退出附身 + zoom out
            // 玩家可以完整看到猫跳上木箱 → 相册掉落 → 小女孩走过来捡起的演出
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.isPossessing && player.currentToy == this)
            {
                player.ExitPossess();
                Debug.Log("[Lamp] Auto-exited possession for ending cinematic.");
            }
        }
    }
}