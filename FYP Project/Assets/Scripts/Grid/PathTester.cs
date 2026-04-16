using System;
using System.Collections.Generic;
using UnityEngine;

// Simple tester to visualise the path between start and goal
public class PathTester : MonoBehaviour
{
    public static Action OnPathUpdated;

    public Transform startMarker;
    public Transform goalMarker;
    public LineRenderer pathLine;

    [Header("Path Display")]
    public bool alwaysShowPath = true;
    public float pathHeight = 0.1f;
    public float gizmoHeight = 0.2f;
    public float lineWidth = 0.15f;

    [Header("Path Animation")]
    public bool animatePath = true;
    public float pathScrollSpeed = 1.5f;

    private List<GridNode> currentPath;

    void Start()
    {
        if (pathLine != null)
        {
            pathLine.startWidth = lineWidth;
            pathLine.endWidth = lineWidth;
        }

        if (alwaysShowPath)
        {
            TestPath();
        }
    }

    void Update()
    {
        if (animatePath && pathLine != null && pathLine.material != null)
        {
            Vector2 offset = pathLine.material.mainTextureOffset;
            offset.x -= pathScrollSpeed * Time.deltaTime;
            pathLine.material.mainTextureOffset = offset;
        }

        // Optional manual path test
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestPath();
        }
    }

    public void TestPath()
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null)
            return;

        if (startMarker == null || goalMarker == null)
            return;

        if (!GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        currentPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        if (currentPath == null)
        {
            if (pathLine != null)
            {
                pathLine.positionCount = 0;
            }

            OnPathUpdated?.Invoke();
            return;
        }

        // Draw path in Game View using LineRenderer
        if (pathLine != null)
        {
            pathLine.startWidth = lineWidth;
            pathLine.endWidth = lineWidth;
            pathLine.positionCount = currentPath.Count;

            for (int i = 0; i < currentPath.Count; i++)
            {
                Vector3 pos = currentPath[i].worldPosition + Vector3.up * pathHeight;
                pathLine.SetPosition(i, pos);
            }
        }

        OnPathUpdated?.Invoke();
    }

    public List<GridNode> GetCurrentPath()
    {
        return currentPath;
    }

    // Debug path visualization in Scene view
    void OnDrawGizmos()
    {
        if (currentPath == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 pos = currentPath[i].worldPosition + Vector3.up * gizmoHeight;
            Gizmos.DrawSphere(pos, 0.1f);

            if (i < currentPath.Count - 1)
            {
                Vector3 next = currentPath[i + 1].worldPosition + Vector3.up * gizmoHeight;
                Gizmos.DrawLine(pos, next);
            }
        }
    }
}