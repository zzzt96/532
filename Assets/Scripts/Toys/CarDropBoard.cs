using UnityEngine;

public class CarDropBoard : MonoBehaviour
{
    [Header("Car to Drop")]
    public GameObject toyCar;
    public float dropForce = 10f;           // 水杯砸的默认力度（重）
    public Vector3 dropDirection = new Vector3(-1f, -0.5f, 0f);

    [Header("State")]
    public bool hasTriggered = false;
    
    public void TriggerDrop()
    {
        TriggerDrop(dropForce);
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

        Rigidbody carRb = toyCar.GetComponent<Rigidbody>();
        if (carRb == null)
        {
            carRb = toyCar.AddComponent<Rigidbody>();
        }

        carRb.isKinematic = false;
        carRb.useGravity = true;
        carRb.interpolation = RigidbodyInterpolation.Interpolate;
        carRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        carRb.constraints = RigidbodyConstraints.FreezePositionZ
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY;

        carRb.AddForce(dropDirection.normalized * force, ForceMode.Impulse);

        Debug.Log($"[CarDropBoard] Car dropped with force={force}!");

        ToyCar car = toyCar.GetComponent<ToyCar>();
        if (car != null)
        {
            car.Drop();
        }
    }
}