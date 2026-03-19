using System.Collections.Generic;
using UnityEngine;

// Handles creation and management of the grid used for building and pathfinding
public class GridManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public Vector3 originPosition = Vector3.zero;

    public float turretHeightOffset = 1f;
    public float trapHeightOffset = 0.15f;

    private GridNode[,] grid;

    public static GridManager Instance;

    private void Awake()
    {
        Instance = this;
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new GridNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);
                grid[x, y] = new GridNode(x, y, worldPos);
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return originPosition + new Vector3(
            x * cellSize + cellSize * 0.5f,
            0f,
            y * cellSize + cellSize * 0.5f
        );
    }

    public bool GetXY(Vector3 worldPosition, out int x, out int y)
    {
        Vector3 local = worldPosition - originPosition;

        x = Mathf.FloorToInt(local.x / cellSize);
        y = Mathf.FloorToInt(local.z / cellSize);

        return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
    }

    public GridNode GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            return null;

        return grid[x, y];
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        if (node.y + 1 < gridHeight)
            neighbours.Add(grid[node.x, node.y + 1]);

        if (node.x + 1 < gridWidth)
            neighbours.Add(grid[node.x + 1, node.y]);

        if (node.y - 1 >= 0)
            neighbours.Add(grid[node.x, node.y - 1]);

        if (node.x - 1 >= 0)
            neighbours.Add(grid[node.x - 1, node.y]);

        return neighbours;
    }

    public bool IsCellBlocked(int x, int y)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return true;

        return node.isBlocked;
    }

    public void SetCellBlocked(int x, int y, bool blocked)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return;

        node.isBlocked = blocked;
    }

    public bool HasWall(int x, int y)
    {
        GridNode node = GetNode(x, y);
        return node != null && node.wallObject != null;
    }

    public bool HasTurret(int x, int y)
    {
        GridNode node = GetNode(x, y);
        return node != null && node.turretObject != null;
    }

    public bool HasTrap(int x, int y)
    {
        GridNode node = GetNode(x, y);
        return node != null && node.trapObject != null;
    }

    public bool PlaceWall(int x, int y, GameObject wallPrefab, int cost)
    {
        GridNode node = GetNode(x, y);

        if (node == null || node.wallObject != null)
            return false;

        GameObject wall = Instantiate(wallPrefab, node.worldPosition, Quaternion.identity);

        node.wallObject = wall;
        node.wallCost = cost;
        node.isBlocked = true;

        return true;
    }

    public bool PlaceTurret(int x, int y, GameObject turretPrefab, int cost)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return false;

        if (node.wallObject == null)
            return false;

        if (node.turretObject != null)
            return false;

        Vector3 turretPosition = node.worldPosition + new Vector3(0f, turretHeightOffset, 0f);
        GameObject turret = Instantiate(turretPrefab, turretPosition, Quaternion.identity);

        node.turretObject = turret;
        node.turretCost = cost;

        return true;
    }

    public bool PlaceTrap(int x, int y, GameObject trapPrefab, int cost)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return false;

        if (node.wallObject != null)
            return false;

        if (node.trapObject != null)
            return false;

        Vector3 trapPosition = node.worldPosition + new Vector3(0f, trapHeightOffset, 0f);
        GameObject trap = Instantiate(trapPrefab, trapPosition, Quaternion.identity);

        node.trapObject = trap;
        node.trapCost = cost;

        return true;
    }

    public int ClearCell(int x, int y)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return 0;

        int totalRefundValue = 0;

        if (node.trapObject != null)
        {
            Destroy(node.trapObject);
            node.trapObject = null;
            totalRefundValue += Mathf.RoundToInt(node.trapCost * 0.5f);
            node.trapCost = 0;
        }

        if (node.turretObject != null)
        {
            Destroy(node.turretObject);
            node.turretObject = null;
            totalRefundValue += Mathf.RoundToInt(node.turretCost * 0.5f);
            node.turretCost = 0;
        }

        if (node.wallObject != null)
        {
            Destroy(node.wallObject);
            node.wallObject = null;
            totalRefundValue += Mathf.RoundToInt(node.wallCost * 0.5f);
            node.wallCost = 0;
        }

        node.isBlocked = false;

        return totalRefundValue;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = originPosition + new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    0f,
                    y * cellSize + cellSize * 0.5f
                );

                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.05f, cellSize));
            }
        }
    }
}