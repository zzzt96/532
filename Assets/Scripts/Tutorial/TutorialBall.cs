using UnityEngine;
using System.Collections;

public class TutorialBall : ToyBase
{
    [Header("References")]
    public TutorialManager tutorialManager;
    public Transform tableEdgeTarget;   // 桌子边缘的Empty GameObject
    public Transform basketTarget;      // 篮子中心的Empty GameObject

    [Header("Roll to Edge")]
    public float rollDuration = 0.8f;   // 滚到桌子边缘的时间

    [Header("Jump into Basket")]
    public float jumpForce = 6f;        // Space键跳跃力度（纯脚本模拟）
    public float arcHeight = 2f;        // 跳跃最高点
    public float jumpDuration = 0.8f;   // 跳跃动画时长
    public float basketRadius = 0.6f;   // 多近算进篮子

    private bool isRolling = false;
    private bool isJumping = false;
    private bool inBasket = false;

    protected override void Start()
    {
        base.Start();
        canBePossessed = false; // 被撞后才能附身
    }

    // 由 TutorialToy 在距离足够近时调用
    public void OnHitByTrain()
    {
        if (isRolling) return;
        StartCoroutine(RollToEdge());
    }

    IEnumerator RollToEdge()
    {
        isRolling = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = tableEdgeTarget.position;
        float elapsed = 0f;

        Debug.Log("[TutorialBall] Rolling to edge...");

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rollDuration); // 丝滑缓入缓出
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        isRolling = false;

        // 到达桌边后，开放附身
        canBePossessed = true;
        Debug.Log("[TutorialBall] Reached edge, now possessable!");
    }

    // 附身后玩家控制
    public override void ToyUpdate()
    {
        if (isJumping || inBasket) return;

        // Space键跳向篮子
        if (Input.GetKeyDown(KeyCode.Space) && basketTarget != null)
        {
            StartCoroutine(JumpToBasket());
        }
    }

    IEnumerator JumpToBasket()
    {
        isJumping = true;
        canBePossessed = false;

        Vector3 startPos = transform.position;
        Vector3 endPos = basketTarget.position;
        float elapsed = 0f;

        Debug.Log("[TutorialBall] Jumping to basket!");

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight; // 抛物线
            transform.position = pos;

            yield return null;
        }

        transform.position = endPos;
        isJumping = false;
        inBasket = true;

        // 通知完成
        GetComponent<InteractableTag>()?.SetCompleted();

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.ExitPossess();

        if (tutorialManager != null)
            tutorialManager.OnBallInBasket();

        Debug.Log("[TutorialBall] In basket!");
    }
}