using System.Collections.Generic;
using UnityEngine;

// Handles pathfinding across the grid using Breadth-First Search
public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Finds a path from start cell to goal cell
    public List<GridNode> FindPath(int startX, int startY, int goalX, int goalY)
    {
        GridNode startNode = GridManager.Instance.GetNode(startX, startY);
        GridNode goalNode = GridManager.Instance.GetNode(goalX, goalY);

        if (startNode == null || goalNode == null)
            return null;

        // Reset previous pathfinding data
        ResetNodes();

        Queue<GridNode> frontier = new Queue<GridNode>();
        HashSet<GridNode> visited = new HashSet<GridNode>();

        frontier.Enqueue(startNode);
        visited.Add(startNode);

        while (frontier.Count > 0)
        {
            GridNode currentNode = frontier.Dequeue();

            // Stop if goal reached
            if (currentNode == goalNode)
            {
                return BuildPath(goalNode);
            }

            foreach (GridNode neighbour in GridManager.Instance.GetNeighbours(currentNode))
            {
                if (visited.Contains(neighbour))
                    continue;

                if (neighbour.isBlocked)
                    continue;

                neighbour.cameFromNode = currentNode;
                frontier.Enqueue(neighbour);
                visited.Add(neighbour);
            }
        }

        // No path found
        return null;
    }

    // Builds the final path by tracing backward from the goal node
    private List<GridNode> BuildPath(GridNode goalNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = goalNode;

        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.cameFromNode;
        }

        path.Reverse();
        return path;
    }

    // Clears previous pathfinding references
    private void ResetNodes()
    {
        for (int x = 0; x < GridManager.Instance.gridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.gridHeight; y++)
            {
                GridNode node = GridManager.Instance.GetNode(x, y);
                node.cameFromNode = null;
            }
        }
    }
}