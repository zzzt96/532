 using UnityEngine;

public class TabHighlighter : MonoBehaviour
{
    [Header("Key")]
    public KeyCode possessKey = KeyCode.Q;

    [Header("Colors")]
    public Color possessableColor = Color.yellow;
    public Color lockedColor = new Color(0.7f, 0.7f, 0.7f); 
    
    [Header("Range")]
    public float maxDistance = 15f;
    public Transform playerTransform;

    private bool isQPressed = false;

    void Start()
    {
        if (playerTransform == null && Camera.main != null)
            playerTransform = Camera.main.transform;
    }

    void Update()
    {
        bool stateChanged = false;

        if (Input.GetKeyDown(possessKey))     { isQPressed = true;  stateChanged = true; }
        else if (Input.GetKeyUp(possessKey))  { isQPressed = false; stateChanged = true; }

        // Q按住期间每帧刷新，确保canBePossessed变化能被及时检测到
        if (isQPressed || stateChanged) UpdateAllHighlights();
    }

    void UpdateAllHighlights()
    {
        Vector3 checkPos = playerTransform != null ? playerTransform.position : Vector3.zero;

        foreach (var tag in InteractableTag.All)
        {
            if (tag == null) continue;

            // 同步ToyBase的canBePossessed到InteractableTag
            ToyBase toy = tag.GetComponent<ToyBase>();
            if (toy != null) tag.canBePossessed = toy.canBePossessed;

            float dist = Vector3.Distance(checkPos, tag.transform.position);
            bool inRange = dist <= maxDistance;

            tag.UpdateHighlight(inRange && isQPressed, possessableColor, lockedColor);
        }
    }
}