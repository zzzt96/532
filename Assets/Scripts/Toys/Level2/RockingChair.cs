using UnityEngine;
using System.Collections;

/// <summary>
/// 摇椅 - 原地摇动，不改变朝向
/// 摇动后吸引猫过来
/// </summary>
public class RockingChair : MonoBehaviour
{
    [Header("Rocking")]
    public float rockAngle = 4f;
    public float rockSpeed = 1f;
    public float rockDuration = 3f;

    private bool isRocking = false;
    private Quaternion initialRotation;

    void Start()
    {
        // 记录初始旋转，摇动时在此基础上叠加，不改变朝向
        initialRotation = transform.localRotation;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!isRocking) StartRocking();
    }

    public void StartRocking()
    {
        if (isRocking) return;
        isRocking = true;
        StartCoroutine(RockRoutine());
    }

    IEnumerator RockRoutine()
    {
        float elapsed = 0f;
        while (elapsed < rockDuration)
        {
            elapsed += Time.deltaTime;
            // 绕初始朝向的Z轴摇动，不影响Y轴朝向
            float angle = Mathf.Sin(elapsed * rockSpeed * Mathf.PI) * rockAngle;
            transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // 摇完恢复初始旋转
        transform.localRotation = initialRotation;
        isRocking = false;

        Debug.Log("[RockingChair] Attracted the cat!");
        Level2Manager.Instance?.cat?.GoToRockingChair();
    }
}