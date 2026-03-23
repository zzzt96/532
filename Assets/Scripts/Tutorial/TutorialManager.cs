using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro tutorialText;
    public PlayerController player;
    public TutorialToy tutorialToy;

    [Header("Cat Event")]
    public TutorialCatNPC tutorialCat;
    public Transform catTargetPoint;

    [Header("Settings")]
    public float textHideDelay = 4f;
    public float autoAdvanceDelay = 2.5f;

    [Header("Level Exit")]
    public GameObject exitZone;

    private int tutorialStep = 0;
    private bool catTriggered = false;
    private float stepStartTime = 0f;

    void Start()
    {
        tutorialStep = 0;
        stepStartTime = Time.time;
        UpdateText();
    }

    void Update()
    {
        switch (tutorialStep)
        {
            case 0:
            case 1:
            case 2:
            case 4:
                if (Time.time - stepStartTime > autoAdvanceDelay)
                { tutorialStep++; UpdateText(); }
                break;

            case 3: // 只有球进篮才推进，不自动跳过
                break;
        }
    }

    public void OnBallInBasket()
    {
        Debug.Log($"[Tutorial] OnBallInBasket! Step: {tutorialStep}");
        if (catTriggered) return;
        catTriggered = true;

        if (tutorialStep < 3) tutorialStep = 3;

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
                tutorialText.text = "Press <color=#FFD700>[Q]</color> to see interactable objects.\n<color=#FFD700>Yellow</color> = can possess now, <color=#9E9E9E>Gray</color> = not yet.";
                break;
            case 2:
                tutorialText.text = "Hover over the toy and <color=#FFD700>hold left mouse button</color> to possess it.";
                break;
            case 3:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> and <color=#FFD700>[Space]</color> to move.\nPush the ball into the basket to attract the cat!";
                break;
            case 4:
                tutorialText.text = "Great! The cat is distracted.\n<color=#FFD700>Click</color> to exit the toy.";
                break;
            case 5:
                tutorialText.text = "Tutorial complete!\nTry to help the little girl.";
                Invoke("HideText", textHideDelay);
                break;
        }

        stepStartTime = Time.time;
    }

    void HideText()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);

        if (exitZone != null)
        {
            exitZone.SetActive(true);
            Debug.Log("[Tutorial] Exit zone activated!");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnTutorialComplete();
    }
}