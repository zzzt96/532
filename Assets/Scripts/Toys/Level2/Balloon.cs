using UnityEngine;
using System.Collections;

/// <summary>
/// 紫色气球 - 玩家附身后用AD左右移动
/// 移动碰到白色气球触发碰撞 → 吸引猫跳起 → cube掉落到风扇
/// </summary>
public class Balloon : ToyBase
{
    [Header("WASD Movement")]
    public float moveSpeed = 3f;
    [Tooltip("左右移动范围（相对于初始位置）")]
    public float moveRangeX = 3f;

    [Header("White Balloon Trigger")]
    [Tooltip("白色气球的位置（空物体放在白气球处）")]
    public Transform whiteBalloonTrigger;
    [Tooltip("触发距离：紫气球离白气球多近时触发")]
    public float triggerDistance = 1.0f;

    [Header("Hanging Cube")]
    [Tooltip("白色气球下挂的cube")]
    public GameObject hangingCube;
    [Tooltip("cube掉落目标位置（风扇开关上方）")]
    public Transform cubeDropTarget;
    public float dropDuration = 0.5f;

    [Header("Cat Landing")]
    [Tooltip("猫腾空后落在风扇桌上的位置")]
    public Transform catLandingPosition;

    private bool triggered = false;
    private Vector3 startPosition;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        canBePossessed = false;
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input = -1f;

        Vector3 pos = transform.position;
        pos.x += input * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, startPosition.x - moveRangeX, startPosition.x + moveRangeX);
        transform.position = pos;

        // 检测是否碰到白气球
        if (whiteBalloonTrigger != null)
        {
            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(whiteBalloonTrigger.position.x, whiteBalloonTrigger.position.y)
            );

            if (dist <= triggerDistance)
            {
                triggered = true;
                canBePossessed = false;
                GetComponent<InteractableTag>()?.SetCompleted();
                Debug.Log("[Balloon] Hit white balloon!");
                StartCoroutine(TriggerSequence());
            }
        }
    }
    
    IEnumerator TriggerSequence()
    {
        var cat = Level2Manager.Instance?.cat;
        if (cat != null && catLandingPosition != null)
            cat.JumpToPosition(catLandingPosition);

        // 等猫跳到最高点（jumpDuration的一半）触发cube掉落
        float halfJump = cat != null ? cat.jumpDuration * 0.6f : 0.3f;
        yield return new WaitForSeconds(halfJump);
        StartCoroutine(DropCube());
    }
    
    IEnumerator DropCube()
    {
        if (hangingCube == null || cubeDropTarget == null)
        {
            Debug.LogWarning("[Balloon] hangingCube or cubeDropTarget not assigned!");
            Level2Manager.Instance?.OnBalloonTriggeredFan();
            yield break;
        }

        hangingCube.transform.SetParent(null);
        Vector3 startPos = hangingCube.transform.position;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            hangingCube.transform.position = Vector3.Lerp(startPos, cubeDropTarget.position, elapsed / dropDuration);
            yield return null;
        }
        hangingCube.transform.position = cubeDropTarget.position;

        Debug.Log("[Balloon] Cube hit fan!");
        Level2Manager.Instance?.OnBalloonTriggeredFan();
    }
}