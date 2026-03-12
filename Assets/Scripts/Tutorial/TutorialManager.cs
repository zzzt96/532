using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro tutorialText;
    public PlayerController player;
    public TutorialToy tutorialToy;

    [Header("Cat Event")]
    public TutorialCatNPC tutorialCat;  // 替换为新的教程猫咪脚本
    public Transform catTargetPoint;    // 猫咪最终要走到的位置

    [Header("Settings")]
    public float textHideDelay = 4f;

    [Header("Level Exit")]
    public GameObject exitZone; // 拖入你要激活的出口触发器物体

    private int tutorialStep = 0;
    private Vector3 lastMousePos;
    private float mouseMoveAccumulator = 0f;

    private bool catTriggered = false;

    void Start()
    {
        tutorialStep = 0;
        lastMousePos = Input.mousePosition;
        UpdateText();
    }

    void Update()
    {
        switch (tutorialStep)
        {
            case 0: // 移动鼠标
                if (Vector3.Distance(Input.mousePosition, lastMousePos) > 5f)
                    mouseMoveAccumulator += Time.deltaTime;
                lastMousePos = Input.mousePosition;
                if (mouseMoveAccumulator > 1.0f) { tutorialStep++; UpdateText(); }
                break;

            case 1: // 附身小火车
                if (player.isPossessing && player.currentToy == tutorialToy)
                { tutorialStep++; UpdateText(); }
                break;

            case 2: // 球进篮，等待 OnBallInBasket()
                break;

            case 3: // 退出附身
                if (!player.isPossessing)
                {
                    tutorialStep++; UpdateText();
                    if (tutorialToy != null) tutorialToy.canBePossessed = false;
                }
                break;
        }
    }
    
    public void OnBallInBasket()
    {
        Debug.Log($"[Tutorial] OnBallInBasket! Step: {tutorialStep}");
        if (catTriggered) return;
        catTriggered = true;

        // 如果还没到step 2，先跳到step 2再推进
        if (tutorialStep < 2) tutorialStep = 2;
    
        tutorialStep++;
        UpdateText();
        if (tutorialCat != null && catTargetPoint != null)
            tutorialCat.TriggerAttention(catTargetPoint);
    }
    void UpdateText()
    {
        if (tutorialText == null) return;

        switch (tutorialStep)
        {
            case 0:
                tutorialText.text = "Move your <color=#FFD700>Mouse</color> to look around.";
                break;
            case 1:
                tutorialText.text = "Hover over the toy and <color=#FFD700>hold left mouse button</color> to possess it.";
                break;
            case 2:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> and <color=#FFD700>[Space]</color> to move.\nPush the ball into the basket to attract the cat!";
                break;
            case 3:
                tutorialText.text = "Great! The cat is distracted.\n<color=#FFD700>Click</color> to exit the toy.";
                break;
            case 4:
                tutorialText.text = "Tutorial complete!\nTry to help the little girl.";
                Invoke("HideText", textHideDelay);
                break;
        }
    }

    void HideText()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);

        // 【新增】：教程完成，激活出口触发器！
        if (exitZone != null)
        {
            exitZone.SetActive(true);
            Debug.Log("[Tutorial] Exit zone activated!");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnTutorialComplete();
    }
}