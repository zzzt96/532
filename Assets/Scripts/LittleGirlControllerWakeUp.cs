using UnityEngine;

public class LittleGirlControllerWakeUp : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool canMove = false;
    
    [Header("Movement Path")]
    public Transform[] waypoints;  
    private int currentWaypointIndex = 0;
    
    [Header("State")]
    public bool hasWokenUp = false;
    
    void Update()
    {
        if (!canMove || !hasWokenUp) return;
        
        MoveToNextWaypoint();
    }
    
    /// <summary>
    /// 醒来并开始移动
    /// </summary>
    public void WakeUpAndMove()
    {
        if (hasWokenUp) return;
        
        hasWokenUp = true;
        canMove = true;
        currentWaypointIndex = 0;
        
        // Capsule 从躺下变成站立
        transform.rotation = Quaternion.Euler(0, 0, 0);
        
        Debug.Log("[LittleGirl] Woke up! Starting to move.");
    }
    
    void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypointIndex >= waypoints.Length)
        {
            // 到达最后一个点
            ReachedEnd();
            return;
        }
        
        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
        {
            currentWaypointIndex++;
            return;
        }
        
        // 移动向目标
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // 检查是否到达
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            Debug.Log($"[LittleGirl] Reached waypoint {currentWaypointIndex}");
            currentWaypointIndex++;
        }
    }
    
    void ReachedEnd()
    {
        canMove = false;
        Debug.Log("[LittleGirl] Reached the door!");
        
        // 通知 GameManager 游戏完成
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameComplete();
        }
    }
}