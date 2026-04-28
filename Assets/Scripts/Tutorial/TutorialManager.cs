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
    public float autoAdvanceDelay = 10f;   // 每句话停留

    [Header("Scene Transition")]
    public string nextSceneName;

    private int tutorialStep = 0;
    private bool catTriggered = false;
    private float stepStartTime = 0f;
    
    private bool waitingForCat = false;
    private float catArrivedTime = -1f;
    public float catWaitDuration = 1f;    // 猫到位后切换

    void Start()
    {
        tutorialStep = 0;
        stepStartTime = Time.time;
        UpdateText();
    }

    void Update()
    {
        // 等猫到位后计时切换
        if (waitingForCat)
        {
            if (catArrivedTime < 0 && tutorialCat != null && tutorialCat.HasArrived())
            {
                catArrivedTime = Time.time;
            }

            if (catArrivedTime >= 0 && Time.time - catArrivedTime >= catWaitDuration)
            {
                LoadNextScene();
            }
            return;
        }

        switch (tutorialStep)
        {
            case 0:
            case 1:
            case 2:
                if (Time.time - stepStartTime > autoAdvanceDelay)
                { tutorialStep++; UpdateText(); }
                break;

            case 3: // 等玩家附身小火车撞球入篮，不自动跳过
                break;

            case 4: // 提示Shift退出，自动推进
                if (Time.time - stepStartTime > autoAdvanceDelay)
                { tutorialStep++; UpdateText(); }
                break;
        }
    }

    public void OnBallInBasket()
    {
        Debug.Log($"[Tutorial] OnBallInBasket! Step: {tutorialStep}");
        if (catTriggered) return;
        catTriggered = true;

        // 触发猫被吸引 (这个不延迟, 立刻执行)
        if (tutorialCat != null && catTargetPoint != null)
            tutorialCat.TriggerAttention(catTargetPoint);

        // 计算 case 3 已显示的时间, 保证总停留时间 >= autoAdvanceDelay
        float elapsedOnStep3 = Time.time - stepStartTime;
        float remaining = autoAdvanceDelay - elapsedOnStep3;

        if (remaining > 0f)
        {
            // 当前 step 显示不够久, 延迟剩余时间再切
            Invoke(nameof(AdvanceToCatStep), remaining);
        }
        else
        {
            AdvanceToCatStep();
        }
    }

    void AdvanceToCatStep()
    {
        tutorialStep = 4; // 推进到"Shift 退出附身"提示
        UpdateText();
    }

    void UpdateText()
    {
        if (tutorialText == null) return;

        switch (tutorialStep)
        {
            case 0:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> to move around.";
                break;
            case 1:
                tutorialText.text = "Press <color=#FFD700>[Q]</color> to see interactable objects.\n<color=#FFD700>Yellow</color> = can possess now,  <color=#9E9E9E>Gray</color> = not yet.";
                break;
            case 2:
                tutorialText.text = "Move close to the train and <color=#FFD700>hold [Space]</color> to possess it.";
                break;
            case 3:
                tutorialText.text = "Use <color=#FFD700>[W][A][S][D]</color> to drive the train.\nKnock the ball into the basket!\n<color=#888888><size=80%>Tip: Some objects hide special actions. Press [Space] to discover them!</size></color>";
                break;
            case 4:
                tutorialText.text = "Great! The cat is distracted.\nPress <color=#FFD700>[Shift]</color> to exit the train.";
                break;
            case 5:
                tutorialText.text = "Well done! Now help the little girl.";
                waitingForCat = true;
                break;
        }

        stepStartTime = Time.time;
    }

    void LoadNextScene()
    {
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);

        Debug.Log("[Tutorial] Complete! Loading next scene.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}