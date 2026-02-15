using UnityEngine;

public class StopPoint : MonoBehaviour
{
    [Header("Stop Point Settings")]
    public int stopIndex = 0;
    
    public bool isFinalStop = false;

    void OnDrawGizmos()
    {
        Gizmos.color = isFinalStop ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, 
            $"Stop {stopIndex}" + (isFinalStop ? " (Final)" : ""));
#endif
    }
}