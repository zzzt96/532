using UnityEngine;
using System.Collections;

public class IronHanger : MonoBehaviour
{
    [Header("References")]
    public Curtain curtain;
    public Transform rodPivot;
    public CatNPC cat;
    public Transform boardPosition;

    [Header("Rod Rotation")]
    public float rotationAngle = 75f;
    public float rotationDuration = 0.8f;
    public Vector3 rotationAxis = new Vector3(0, 0, 1);

    [Header("Curtain Pull")]
    public float curtainPullDuration = 1.5f;

    [Header("State")]
    public bool hasActivated = false;

    public void Activate() => ActivateSequence();
    public void ActivateBunny() => ActivateSequence();

    private void ActivateSequence()
    {
        if (hasActivated) return;
        hasActivated = true;
        StartCoroutine(RodSwingAndPullCurtain());
        Debug.Log("[IronHanger] Activated!");
    }

    IEnumerator RodSwingAndPullCurtain()
    {
        // ===== 阶段1：木杆旋转下垂 =====
        if (rodPivot != null)
        {
            float rotated = 0f;
            float rotatePerSecond = rotationAngle / rotationDuration;

            while (rotated < rotationAngle)
            {
                float step = rotatePerSecond * Time.deltaTime;
                step = Mathf.Min(step, rotationAngle - rotated);
                rodPivot.Rotate(rotationAxis, step, Space.Self);
                rotated += step;
                yield return null;
            }
            Debug.Log("[IronHanger] Rod dropped.");
        }

        // ===== 阶段2：晃向拉链然后停住 =====
        float swingDuration = 0.6f;
        float swingAngle = 140f;   // 额外晃向拉链的角度
        float swingElapsed = 0f;
        float swingRotated = 0f;

        while (swingElapsed < swingDuration)
        {
            swingElapsed += Time.deltaTime;
            float t = swingElapsed / swingDuration;
            float smooth = Mathf.Sin(t * Mathf.PI * 0.5f); // ease out
            float targetRotated = smooth * swingAngle;
            float delta = targetRotated - swingRotated;
            rodPivot.Rotate(rotationAxis, delta, Space.Self);
            swingRotated = targetRotated;
            yield return null;
        }
        // 停在这里，不归零，视觉上就是勾住了
        Debug.Log("[IronHanger] Hooked!");
        
        yield return new WaitForSeconds(0.5f); 

        // ===== 阶段3：拉开窗帘 =====
        if (curtain != null)
        {
            float elapsed = 0f;
            while (elapsed < curtainPullDuration)
            {
                elapsed += Time.deltaTime;
                curtain.SetOpenProgress(elapsed / curtainPullDuration);
                yield return null;
            }
            curtain.SetOpenProgress(1f);
            Debug.Log("[IronHanger] Curtain fully opened!");

            if (cat != null && boardPosition != null)
            {
                cat.AttractedByIvy(boardPosition.position);
                Debug.Log("[IronHanger] Cat attracted by sunlight!");
            }
        }
        else
        {
            Debug.LogWarning("[IronHanger] Curtain not assigned!");
        }
    }
}