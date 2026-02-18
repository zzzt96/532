using UnityEngine;

public class CarDropBoard : MonoBehaviour
{
    [Header("Car to Drop")]
    public GameObject toyCar;
    public float nudgeForce = 1.5f;     // 轻推力度
    public Vector3 nudgeDirection = new Vector3(-1f, 0f, 0f); // 往左推一点

    [Header("State")]
    public bool hasTriggered = false;

    public void TriggerDrop()
    {
        TriggerDrop(nudgeForce);
    }

    public void TriggerDrop(float force)
    {
        if (hasTriggered) return;
        if (toyCar == null)
        {
            Debug.LogWarning("[CarDropBoard] toyCar is null!");
            return;
        }

        hasTriggered = true;
        
        ToyCar car = toyCar.GetComponent<ToyCar>();
        if (car != null)
        {
            car.ActivateOnBoard();
            Debug.Log($"[CarDropBoard] Car activated on board!");
        }
    }
}