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
            case 0: // Step 0: Teach Mouse
                if (Vector3.Distance(Input.mousePosition, lastMousePos) > 5f)
                {
                    mouseMoveAccumulator += Time.deltaTime;
                }
                lastMousePos = Input.mousePosition;

                if (mouseMoveAccumulator > 1.0f)
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 1: // Step 1: Teach Q
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 2: // Step 2: Teach E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 3: // Step 3: Teach Possess
                if (player.isPossessing && player.currentToy == tutorialToy)
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 4: // Step 4: Push the ball
                // 等待 TutorialBall 调用 OnBallInBasket()
                break;

            case 5: // Step 5: Unpossess
                if (!player.isPossessing)
                {
                    tutorialStep++;
                    UpdateText();

                    if (tutorialToy != null) tutorialToy.canBePossessed = false;
                }
                break;
        }
    }

    public void OnBallInBasket()
    {
        if (tutorialStep == 4)
        {
            tutorialStep++;
            UpdateText();

            // 触发猫咪！
            if (tutorialCat != null && catTargetPoint != null)
            {
                tutorialCat.TriggerAttention(catTargetPoint);
            }
        }
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
                tutorialText.text = "Press <color=#FFD700>[Q]</color> to scan for possessable objects.";
                break;
            case 2:
                tutorialText.text = "Press <color=#FFD700>[E]</color> to scan for interactables.";
                break;
            case 3:
                tutorialText.text = "Hover over the toy and press <color=#FFD700>[Shift]</color> to possess it.";
                break;
            case 4:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> and <color=#FFD700>[Space]</color> to move.\nPush the ball into the basket to attract the cat!";
                break;
            case 5:
                tutorialText.text = "Great! The cat is distracted.\nPress <color=#FFD700>[Shift]</color> to exit the toy.";
                break;
            case 6:
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