using UnityEngine;
using System.Collections;

public class Fan : ToyBase
{
    [Header("Fan Head")]
    public float headRotateSpeed = 50f;
    public float maxHeadAngle = 200f;

    [Header("Fan Blades")]
    public Transform fanBlades;
    public float bladeSpinSpeed = 360f;

    [Header("Toilet Paper")]
    public GameObject toiletPaper;
    public float blowAngle = -180f;
    public float angleTolerance = 15f;
    public float blowHoldTime = 0.8f;

    [Header("Toilet Paper Landing")]
    [Tooltip("厕纸滚落的目标位置（放在第一本书旁边的空物体）")]
    public Transform toiletPaperLandTarget;
    public float rollDuration = 0.6f;
    public float fallDuration = 0.4f;

    [Header("Domino Chain")]
    public DominoChain dominoChain;

    private bool isOn = false;
    private float currentHeadAngle = 0f;
    private bool blowTriggered = false;
    private float blowTimer = 0f;
    private Quaternion initialRotation;

    protected override void Start()
    {
        base.Start();
        initialRotation = transform.localRotation;
        canBePossessed = false;
    }

    void Update()
    {
        if (isOn && fanBlades != null)
            fanBlades.Rotate(Vector3.left,bladeSpinSpeed * Time.deltaTime, Space.Self); // 加了 Space.Self 确保绕自身轴旋转
    }

    public override void ToyUpdate()
    {
        if (!isOn || blowTriggered) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input =  1f;

        currentHeadAngle += input * headRotateSpeed * Time.deltaTime;
        currentHeadAngle = Mathf.Clamp(currentHeadAngle, -maxHeadAngle, maxHeadAngle);
        transform.localRotation = initialRotation * Quaternion.Euler(0f, currentHeadAngle, 0f);

        bool aimed = Mathf.Abs(currentHeadAngle - blowAngle) <= angleTolerance;
        blowTimer = aimed ? blowTimer + Time.deltaTime : 0f;

        if (blowTimer >= blowHoldTime)
        {
            blowTriggered = true;
            canBePossessed = false;
            GetComponent<InteractableTag>()?.SetCompleted();
            StartCoroutine(BlowToiletPaper());
        }
    }

    public void TurnOn()
    {
        isOn = true;
        Debug.Log("[Fan] Turned on!");
    }

    IEnumerator BlowToiletPaper()
    {
        if (toiletPaper == null)
        {
            dominoChain?.StartChain();
            yield break;
        }

        Debug.Log("[Fan] Blowing toilet paper!");

        Vector3 startPos = toiletPaper.transform.position;

        // 如果没有指定落点，就往左滚1.5单位后落地
        Vector3 landPos = toiletPaperLandTarget != null
            ? toiletPaperLandTarget.position
            : new Vector3(startPos.x - 1.5f, 0.15f, startPos.z);

        // 中间经过桌子边缘（和起点同高，落点X位置）
        Vector3 edgePos = new Vector3(landPos.x, startPos.y, startPos.z);

        // 滚到桌边
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rollDuration);
            toiletPaper.transform.position = Vector3.Lerp(startPos, edgePos, t);
            toiletPaper.transform.Rotate(Vector3.forward, -300f * Time.deltaTime);
            yield return null;
        }
        toiletPaper.transform.position = edgePos;

        // 落到地面
        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            toiletPaper.transform.position = Vector3.Lerp(edgePos, landPos, t);
            yield return null;
        }
        toiletPaper.transform.position = landPos;

        // 落地后禁用重力防止继续掉落
        Rigidbody rb = toiletPaper.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("[Fan] Toilet paper landed! Starting domino chain.");
        dominoChain?.StartChain();
    }
}