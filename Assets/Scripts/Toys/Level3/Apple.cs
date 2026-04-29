using System.Collections;
using UnityEngine;

public class Apple : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float minX = -60f;
    public float maxX = -40f;
    public float minZ = -10f;
    public float maxZ = 0f;

    [Header("Trigger Zone")]
    public Transform candlePosition;
    public float triggerRadius = 1.5f;

    [Header("Shadow")]
    public GameObject roundShadow;
    public float shadowFadeDuration = 1f;

    // ==================== Audio ====================
    [Header("Audio")]
    [Tooltip("金属在轨道上滑动的'哗啦/咔啦'声 (按 WASD 移动时持续 loop)")]
    public SoundSlot slideRailSound;
    // ===============================================

    float fixedY;
    bool triggered = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        fixedY = transform.position.y;

        if (roundShadow != null)
        {
            var r = roundShadow.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = 0f;
                r.material.color = c;
            }
        }
    }

    public override void UnPossess()
    {
        base.UnPossess();
        // 玩家退出附身时停止滑动声
        StopSound();
    }

    public override void ToyUpdate()
    {
        if (triggered) return;

        float moveX = 0f;
        float moveZ = 0f;
        if (Input.GetKey(KeyCode.W)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.S)) moveZ += 1f;
        if (Input.GetKey(KeyCode.A)) moveX += 1f;
        if (Input.GetKey(KeyCode.D)) moveX -= 1f;

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + move;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = fixedY;
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        transform.position = newPos;

        // 移动时持续播滑动 loop, 不动时停止
        bool isMoving = (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f);
        if (isMoving)
            PlaySound(slideRailSound);
        else
            StopSound();

        if (candlePosition != null)
        {
            float dist = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(candlePosition.position.x, 0, candlePosition.position.z)
            );
            if (dist <= triggerRadius)
            {
                triggered = true;
                Debug.Log("[Apple] Covered candle! Triggering shadow change.");

                // 停止滑动音
                StopSound();

                StartCoroutine(FadeInShadow());
                Level3Manager.Instance?.OnAppleCoveredCandle();

                // zoom out 修复: 苹果到位, 玩家退出附身看影子变化
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && player.isPossessing && player.currentToy == this)
                {
                    player.ExitPossess();
                    Debug.Log("[Apple] Auto-exited possession.");
                }
            }
        }
    }

    IEnumerator FadeInShadow()
    {
        if (roundShadow == null) yield break;

        roundShadow.SetActive(true);
        var r = roundShadow.GetComponent<Renderer>();
        if (r == null) yield break;

        float elapsed = 0f;
        Color c = r.material.color;
        while (elapsed < shadowFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / shadowFadeDuration);
            r.material.color = c;
            yield return null;
        }
        c.a = 1f;
        r.material.color = c;
        Debug.Log("[Apple] Round shadow fully visible.");
    }

    public override void Possess()
    {
        base.Possess();
        Debug.Log("[Apple] Possessed - Use WASD to move in front of the candle!");
    }
}