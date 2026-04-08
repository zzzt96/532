using System.Collections;
using UnityEngine;

public class Apple : ToyBase
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    [Tooltip("移动范围")]
    public float minX = -60f;
    public float maxX = -40f;
    public float minZ = -10f;
    public float maxZ = 0f;

    [Header("Trigger Zone")]
    [Tooltip("蜡烛前方的触发位置（拖入空物体或蜡烛Transform）")]
    public Transform candlePosition;
    [Tooltip("距离蜡烛多近时触发")]
    public float triggerRadius = 1.5f;

    [Header("Shadow")]
    [Tooltip("圆形影子GameObject（初始invisible的Quad或Sprite）")]
    public GameObject roundShadow;
    [Tooltip("影子fade in持续时间")]
    public float shadowFadeDuration = 1f;

    float fixedY;
    bool triggered = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false;
        fixedY = transform.position.y;

        // 初始隐藏圆形影子
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

    public override void ToyUpdate()
    {
        if (triggered) return;

        // WASD移动，固定Y轴
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

        // 检测是否到达蜡烛前方
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
                StartCoroutine(FadeInShadow());
                Level3Manager.Instance?.OnAppleCoveredCandle();
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