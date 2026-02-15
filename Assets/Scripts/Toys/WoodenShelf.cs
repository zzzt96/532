using UnityEngine;

public class WoodenShelf : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 80f;
    public float targetAngle = 35f;         // 正数，倒多少度
    public bool fallLeft = true;            // true=向左倒（朝小架子），false=向右倒

    [Header("References")]
    public GameObject toyRocket;

    [Header("State")]
    public bool hasKnockedDown = false;
    private bool isFalling = false;
    private float currentAngle = 0f;

    public void KnockDown()
    {
        if (hasKnockedDown) return;
        hasKnockedDown = true;
        isFalling = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("[WoodenShelf] Knocked down! Starting to fall...");
    }

    void Update()
    {
        if (!isFalling) return;

        float step = fallSpeed * Time.deltaTime;
        currentAngle += step;

        // fallLeft=true 向左倒，fallLeft=false 向右倒
        float direction = fallLeft ? 1f : -1f;
        transform.Rotate(Vector3.forward, step * direction, Space.World);

        if (currentAngle >= targetAngle)
        {
            isFalling = false;
            Debug.Log("[WoodenShelf] Fell down!");
            ActivateRocket();
        }
    }

    void ActivateRocket()
    {
        if (toyRocket == null)
        {
            Debug.LogWarning("[WoodenShelf] toyRocket is null!");
            return;
        }

        ToyRocket rocket = toyRocket.GetComponent<ToyRocket>();
        if (rocket != null)
        {
            rocket.Activate();
        }

        Debug.Log("[WoodenShelf] Rocket activated!");
    }
}