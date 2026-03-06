using System.Collections.Generic;
using UnityEngine;

// Simple tester to visualise the path between start and goal
public class PathTester : MonoBehaviour
{
    public Transform startMarker;
    public Transform goalMarker;

    public LineRenderer pathLine;

    private List<GridNode> currentPath;

    void Update()
    {
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

        if (!GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        currentPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        if (currentPath == null)
        {
            Debug.Log("No path found.");

            if (pathLine != null)
            {
                pathLine.positionCount = 0;
            }

            return;
        }

        Debug.Log("Path found. Length: " + currentPath.Count);

        // Draw path in Game View using LineRenderer
        if (pathLine != null)
        {
            pathLine.positionCount = currentPath.Count;

            for (int i = 0; i < currentPath.Count; i++)
            {
                Vector3 pos = currentPath[i].worldPosition + Vector3.up * 0.1f;
                pathLine.SetPosition(i, pos);
            }
        }
    }

    // Debug path visualization in Scene view
    void OnDrawGizmos()
    {
        if (currentPath == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 pos = currentPath[i].worldPosition + Vector3.up * 0.2f;

            Gizmos.DrawSphere(pos, 0.15f);

            if (i < currentPath.Count - 1)
            {
                Vector3 next = currentPath[i + 1].worldPosition + Vector3.up * 0.2f;
                Gizmos.DrawLine(pos, next);
            }
        }
    }
}