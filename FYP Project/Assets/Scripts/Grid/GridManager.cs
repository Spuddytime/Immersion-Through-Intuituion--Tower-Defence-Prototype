using UnityEngine;

// Handles creation and management of the grid used for building and pathfinding
public class GridManager : MonoBehaviour
{
    // Size of the grid (number of cells)
    public int gridWidth = 20;
    public int gridHeight = 20;

    // Physical size of each grid cell in world units
    public float cellSize = 1f;

    // World position where the grid starts
    public Vector3 originPosition = Vector3.zero;

    // 2D array storing all grid nodes
    private GridNode[,] grid;

    // Singleton reference so other scripts can easily access the grid
    public static GridManager Instance;

    private void Awake()
    {
        // Assign this instance
        Instance = this;

        // Generate the grid when the scene starts
        CreateGrid();
    }

    // Creates the full grid of nodes
    void CreateGrid()
    {
        grid = new GridNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);

                // Create a node for this cell
                grid[x, y] = new GridNode(x, y, worldPos);
            }
        }
    }

    // Converts grid coordinates into a world position
    public Vector3 GetWorldPosition(int x, int y)
    {
        return originPosition +
               new Vector3(
                   x * cellSize + cellSize * 0.5f,
                   0f,
                   y * cellSize + cellSize * 0.5f
               );
    }

    // Converts a world position into grid coordinates
    public bool GetXY(Vector3 worldPosition, out int x, out int y)
    {
        Vector3 local = worldPosition - originPosition;

        x = Mathf.FloorToInt(local.x / cellSize);
        y = Mathf.FloorToInt(local.z / cellSize);

        // Returns true if the position is inside the grid
        return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
    }

    // Places an object (like a wall or tower) in a grid cell
    public bool PlaceObject(int x, int y, GameObject prefab)
    {
        GridNode node = grid[x, y];

        // Prevent placing objects in blocked cells
        if (node.isBlocked)
            return false;

        // Instantiate object at cell position
        GameObject obj = Instantiate(prefab, node.worldPosition, Quaternion.identity);

        // Mark the cell as occupied
        node.isBlocked = true;
        node.placedObject = obj;

        return true;
    }

    // Removes any object from the specified cell
    public void ClearCell(int x, int y)
    {
        GridNode node = grid[x, y];

        if (node.placedObject != null)
            Destroy(node.placedObject);

        node.isBlocked = false;
        node.placedObject = null;
    }

    // Draws the grid in the Scene view for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos =
                    originPosition +
                    new Vector3(
                        x * cellSize + cellSize * 0.5f,
                        0f,
                        y * cellSize + cellSize * 0.5f
                    );

                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.05f, cellSize));
            }
        }
    }
}