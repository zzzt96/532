using UnityEngine;
using System.Collections;

/// <summary>
/// 气球 - 玩家附身后按 E/Space 晃动
/// 晃动满次数后，挂着的物体掉落到风扇开关位置，触发风扇启动
/// 和 Level 1 的 IvyPlant 晃动逻辑相同
/// </summary>
public class Balloon : ToyBase
{
    [Header("Shake")]
    public float shakeAngle = 20f;
    public float shakeDuration = 0.35f;
    [Tooltip("需要晃动几次才触发掉落")]
    public int requiredShakes = 2;

    [Header("Hanging Object")]
    [Tooltip("气球下方挂着的物体，满次数后掉落")]
    public GameObject hangingObject;
    [Tooltip("掉落的目标位置（风扇开关上方的空物体）")]
    public Transform dropTarget;
    public float dropDuration = 0.5f;

    private int shakeCount = 0;
    private bool isShaking = false;
    private bool dropped = false;

    public override void ToyUpdate()
    {
        if (dropped || isShaking) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin((elapsed / shakeDuration) * Mathf.PI * 2f) * shakeAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        isShaking = false;
        shakeCount++;

        Debug.Log($"[Balloon] Shake {shakeCount}/{requiredShakes}");

        if (shakeCount >= requiredShakes)
        {
            dropped = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            StartCoroutine(DropHangingObject());
        }
    }

    IEnumerator DropHangingObject()
    {
        if (hangingObject == null || dropTarget == null)
        {
            Debug.LogWarning("[Balloon] hangingObject or dropTarget not assigned!");
            Level2Manager.Instance?.OnBalloonTriggeredFan();
            yield break;
        }

        hangingObject.transform.SetParent(null);
        Vector3 startPos = hangingObject.transform.position;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            hangingObject.transform.position = Vector3.Lerp(startPos, dropTarget.position, elapsed / dropDuration);
            yield return null;
        }
        hangingObject.transform.position = dropTarget.position;

        Debug.Log("[Balloon] Object hit fan switch!");
        Level2Manager.Instance?.OnBalloonTriggeredFan();
    }
}