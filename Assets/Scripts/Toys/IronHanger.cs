using UnityEngine;

public class IronHanger : MonoBehaviour
{
    [Header("References")]
    public BunnyToy bunnyToy;
    public GameObject cylinderToBreak;

    [Header("State")]
    public bool hasActivated = false;

    // ★ Ball 的 SendMessage("Activate") 会调用这个
    public void Activate()
    {
        ActivateSequence();
    }

    // ★ Ball 的 SendMessage("ActivateBunny") 也兼容
    public void ActivateBunny()
    {
        ActivateSequence();
    }

    private void ActivateSequence()
    {
        if (hasActivated) return;
        hasActivated = true;

        if (cylinderToBreak != null)
        {
            cylinderToBreak.SetActive(false);
        }

        if (bunnyToy != null)
        {
            bunnyToy.Activate();
            Debug.Log("[IronHanger] Activated: bunny swing started!");
        }
        else
        {
            Debug.LogWarning("[IronHanger] bunnyToy is null! Drag BunnyToy into Inspector.");
        }
    }
}