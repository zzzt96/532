using UnityEngine;

public class PossessUI : MonoBehaviour
{
    public static PossessUI Instance;
    void Awake() { Instance = this; }
    public void Show(Vector3 worldPos, float progress) { }
    public void Hide() { }
}