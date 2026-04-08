using UnityEngine;

public class AirPump : ToyBase
{
    [Header("Pump Settings")]
    [Tooltip("充满需要持续按住Space多少秒")]
    public float fillDuration = 3.5f;

    [Header("Pump Animation")]
    [Tooltip("打气筒活塞Transform（可选，按压动画）")]
    public Transform piston;
    [Tooltip("活塞按压距离")]
    public float pistonPressDistance = 0.3f;

    [Header("Balloon Reference")]
    public BalloonL3 balloon;

    float fillTimer = 0f;
    bool filled = false;
    Vector3 pistonStartPos;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        if (piston != null) pistonStartPos = piston.localPosition;
    }

    public override void ToyUpdate()
    {
        if (filled) return;

        if (Input.GetKey(KeyCode.Space))
        {
            fillTimer += Time.deltaTime;

            // 活塞按压动画
            if (piston != null)
            {
                float press = Mathf.Sin(Time.time * 8f) * pistonPressDistance * 0.5f + pistonPressDistance * 0.5f;
                piston.localPosition = pistonStartPos + Vector3.left * press;
            }

            float progress = Mathf.Clamp01(fillTimer / fillDuration);
            balloon?.UpdateInflationProgress(progress);
            // Debug.Log($"[AirPump] Filling... {progress * 100f:F0}%");

            if (fillTimer >= fillDuration)
            {
                filled = true;
                if (piston != null) piston.localPosition = pistonStartPos;
                Debug.Log("[AirPump] Balloon filled!");
                Level3Manager.Instance?.OnBalloonFilled();
            }
        }
        else
        {
            // 松开时活塞复位
            if (piston != null)
                piston.localPosition = Vector3.Lerp(piston.localPosition, pistonStartPos, Time.deltaTime * 10f);
        }
    }

    public override void Possess()
    {
        base.Possess();
        fillTimer = 0f;
        Debug.Log("[AirPump] Possessed - Hold Space to pump!");
    }

    public override void UnPossess()
    {
        base.UnPossess();
        if (piston != null) piston.localPosition = pistonStartPos;
    }
}