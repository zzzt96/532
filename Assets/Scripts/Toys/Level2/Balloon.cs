using UnityEngine;
using System.Collections;

public class Balloon : ToyBase
{
    [Header("WASD Movement")]
    public float moveSpeed = 3f;
    public float moveRangeX = 3f;

    [Header("White Balloon Trigger")]
    public Transform whiteBalloonTrigger;
    public float triggerDistance = 1.0f;

    [Header("Hanging Cube")]
    public GameObject hangingCube;
    public Transform cubeDropTarget;
    public float dropDuration = 0.5f;

    [Header("Cat Landing")]
    public Transform catLandingPosition;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("气球移动空气摩擦呼呼声 (按 AD 移动时持续 loop)")]
    public SoundSlot balloonMoveSound;

    [Tooltip("撞白气球瞬间的橡胶拉扯嗖+吱声")]
    public SoundSlot balloonPullSound;

    [Tooltip("金属cube掉到风扇开关上的清脆当啷声")]
    public SoundSlot metalDropSound;
    // ===============================================

    private bool triggered = false;
    private Vector3 startPosition;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        canBePossessed = false;
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 玩家退出附身时停止移动 loop 音
        StopSound();
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        // 只接受 WASD, 不接受方向键
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = 1f;
        if (Input.GetKey(KeyCode.D)) input = -1f;

        Vector3 pos = transform.position;
        pos.x += input * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, startPosition.x - moveRangeX, startPosition.x + moveRangeX);
        transform.position = pos;

        // 移动中持续播 loop, 不动时停
        if (Mathf.Abs(input) > 0.01f)
            PlaySound(balloonMoveSound);
        else
            StopSound();

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

                // 停止移动音, 播撞击声
                StopSound();
                PlaySound(balloonPullSound);

                StartCoroutine(TriggerSequence());
            }
        }
    }

    IEnumerator TriggerSequence()
    {
        // zoom out 修复: 触发演出后让玩家退出附身, 看猫腾空 + cube 掉落
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.isPossessing && player.currentToy == this)
        {
            player.ExitPossess();
            Debug.Log("[Balloon] Auto-exited possession for cinematic.");
        }

        var cat = Level2Manager.Instance?.cat;
        if (cat != null && catLandingPosition != null)
            cat.JumpToPosition(catLandingPosition);

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

        // 金属cube落地瞬间
        PlaySound(metalDropSound);

        Debug.Log("[Balloon] Cube hit fan!");
        Level2Manager.Instance?.OnBalloonTriggeredFan();
    }
}