using System.Collections.Generic;
using UnityEngine;

// Simple tester to visualise the path between start and goal
public class PathTester : MonoBehaviour
{
    public Transform startMarker;
    public Transform goalMarker;

    private List<GridNode> currentPath;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestPath();
        }
    }

     public void TestPath()
    {
        if (!GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        currentPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        if (currentPath == null)
        {
            Debug.Log("No path found.");
        }
        else        
        {
            Debug.Log("Path found. Length: " + currentPath.Count);
        }
    }

    private void OnDrawGizmos()
    {
        if (currentPath == null) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[i].worldPosition + Vector3.up * 0.2f, 0.15f);

            if (i < currentPath.Count - 1)
            {
                Gizmos.DrawLine(
                    currentPath[i].worldPosition + Vector3.up * 0.2f,
                    currentPath[i + 1].worldPosition + Vector3.up * 0.2f
                );
            }
        }
    }
}