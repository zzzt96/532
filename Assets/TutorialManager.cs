using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro tutorialText;
    public PlayerController player;
    public TutorialToy tutorialToy;

    [Header("Settings")]
    public float textHideDelay = 3f;

    private int tutorialStep = 0;
    private float moveTimer = 0f;
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
            case 0: // Step 0: Teach Mouse Movement
                // �������Ƿ�����Ļ���ƶ�
                if (Vector3.Distance(Input.mousePosition, lastMousePos) > 5f)
                {
                    mouseMoveAccumulator += Time.deltaTime;
                }
                lastMousePos = Input.mousePosition;

                // �ۼ��ƶ����1�����ҽ�����һ��
                if (mouseMoveAccumulator > 1.0f)
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 1: // Step 1: Teach Tab Highlight
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 2: // Step 2: Teach Possession
                if (player.isPossessing && player.currentToy == tutorialToy)
                {
                    tutorialStep++;
                    UpdateText();
                }
                break;

            case 3: // Step 3: Teach WASD Movement
                if (player.isPossessing && player.currentToy == tutorialToy)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    {
                        moveTimer += Time.deltaTime;
                    }

                    // ���ʹ�� WASD �ƶ��� 1 ���Ӻ�����ʾ����
                    if (moveTimer > 1.0f)
                    {
                        tutorialStep++;
                        UpdateText();
                    }
                }
                break;

            case 4: // Step 4: Teach Unpossess
                if (!player.isPossessing)
                {
                    tutorialStep++;
                    UpdateText();

                    // �̳̽������ѽ�ѧ�������Ϊ���ɸ�����ֹ���ź�����Ϸ
                    if (tutorialToy != null) tutorialToy.canBePossessed = false;
                    var tag = tutorialToy.GetComponent<InteractableTag>();
                    if (tag != null) tag.enabled = false;
                }
                break;
        }
    }

    void UpdateText()
    {
        if (tutorialText == null) return;

        switch (tutorialStep)
        {
            case 0:
                tutorialText.text = "Move your <color=#FFD700>Mouse</color> to look around";
                break;
            case 1:
                tutorialText.text = "Press <color=#FFD700>[TAB]</color> to scan for interactable objects";
                break;
            case 2:
                tutorialText.text = "Hover over the object and press <color=#FFD700>[Shift]</color> to possess it";
                break;
            case 3:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> to move the object";
                break;
            case 4:
                tutorialText.text = "Press <color=#FFD700>[Shift]</color> to exit the object";
                break;
            case 5:
                tutorialText.text = "Tutorial complete!\nExplore the room.";
                Invoke("HideText", textHideDelay);
                break;
        }
    }

    void HideText()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);

        // Tutorial 结束，触发过场镜头
        if (GameManager.Instance != null)
            GameManager.Instance.OnTutorialComplete();
    }
}