using UnityEngine;
using System.Collections;

public class TutorialCatNPC : MonoBehaviour
{
    [Header("References")]
    public Animation catAnimation;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float turnDuration = 0.5f;

    [Header("Animation Clip Names")]
    public string clipIdle = "Idle";
    public string clipWalk = "Walk";
    public string clipTurn = "Turn";
    public string clipFinal = "FinalAction";

    private Vector3 targetPos;
    private bool isMoving = false;
    private float currentFaceAngle = 90f;

    void Start()
    {
        // 记录初始朝向，防止翻转乱套
        currentFaceAngle = transform.eulerAngles.y;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;

        if (catAnimation != null) catAnimation.CrossFade(clipIdle, 0.1f);
    }

    void Update()
    {
        if (isMoving) MoveToTarget();
    }

    void LateUpdate()
    {
        // 绝对控制旋转，对抗旧版动画的错误数据
        transform.eulerAngles = new Vector3(0, currentFaceAngle, 0);
    }

    // 由 TutorialManager 调用
    public void TriggerAttention(Transform target)
    {
        if (isMoving) return;

        targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        float targetAngle = targetPos.x > transform.position.x ? 90f : -90f;

        StartCoroutine(TurnAndWalkRoutine(targetAngle));
    }

    IEnumerator TurnAndWalkRoutine(float targetAngle)
    {
        if (catAnimation != null) catAnimation.CrossFade(clipTurn, 0.2f);

        float startAngle = currentFaceAngle;
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnDuration;
            currentFaceAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            yield return null;
        }

        currentFaceAngle = targetAngle;
        isMoving = true;
        if (catAnimation != null) catAnimation.CrossFade(clipWalk, 0.2f);
    }

    void MoveToTarget()
    {
        float distX = Mathf.Abs(targetPos.x - transform.position.x);

        if (distX <= 0.1f) // 到达目标点
        {
            isMoving = false;
            if (catAnimation != null) catAnimation.CrossFade(clipFinal, 0.2f);
            return;
        }

        float zDiff = targetPos.z - transform.position.z;
        float zStep = Mathf.MoveTowards(0, zDiff, walkSpeed * Time.deltaTime);

        float dirX = targetPos.x > transform.position.x ? 1f : -1f;
        transform.position += new Vector3(dirX * walkSpeed * Time.deltaTime, 0, zStep);

        currentFaceAngle = dirX > 0 ? 90f : -90f;
    }
}