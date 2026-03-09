using System.Collections.Generic;
using UnityEngine;

// Moves an enemy along a path and recalculates if the maze changes
public class EnemyMover : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public Transform goalMarker;
    public int damageToBase = 1;

    [Header("Visuals")]
    public Transform visualRoot;
    public Vector3 visualRotationOffset = Vector3.zero;

    private List<GridNode> path;
    private int currentPathIndex = 0;
    private BaseHealth baseHealth;

    private void OnEnable()
    {
        PathTester.OnPathUpdated += RecalculatePath;
    }

    private void OnDisable()
    {
        PathTester.OnPathUpdated -= RecalculatePath;
    }

    private void Start()
    {
        baseHealth = FindFirstObjectByType<BaseHealth>();

        // Apply the visual offset once at start
        if (visualRoot != null)
        {
            visualRoot.localRotation = Quaternion.Euler(visualRotationOffset);
        }
    }

    public void SetPath(List<GridNode> newPath)
    {
        path = newPath;
        currentPathIndex = 0;

        if (path != null && path.Count > 0)
        {
            transform.position = path[0].worldPosition + Vector3.up * 0.5f;
        }
    }

    void Update()
    {
        if (path == null || path.Count == 0)
            return;

        if (currentPathIndex >= path.Count)
            return;

        Vector3 targetPosition = path[currentPathIndex].worldPosition + Vector3.up * 0.5f;
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0f;

        // Rotate the root for movement
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Keep the visual child in the correct local orientation
        if (visualRoot != null)
        {
            visualRoot.localRotation = Quaternion.Euler(visualRotationOffset);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex >= path.Count)
            {
                ReachGoal();
            }
        }
    }

    void ReachGoal()
    {
        Debug.Log("Enemy reached the goal.");

        if (baseHealth != null)
        {
            baseHealth.TakeDamage(damageToBase);
        }

        Destroy(gameObject);
    }

    void RecalculatePath()
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null || goalMarker == null)
            return;

        if (!GridManager.Instance.GetXY(transform.position, out int currentX, out int currentY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        List<GridNode> newPath = Pathfinder.Instance.FindPath(currentX, currentY, goalX, goalY);

        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            currentPathIndex = 0;
        }
    }
}