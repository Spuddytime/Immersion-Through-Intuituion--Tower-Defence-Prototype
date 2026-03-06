using UnityEngine;

// Represents a single cell in the grid
public class GridNode
{
    // Grid coordinates
    public int x;
    public int y;

    // Whether this cell is blocked by a placed object
    public bool isBlocked;

    // World position of the centre of the cell
    public Vector3 worldPosition;

    // Reference to any placed object in this cell
    public GameObject placedObject;

    // Used by pathfinding to reconstruct the path
    public GridNode cameFromNode;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;

        isBlocked = false;
        placedObject = null;
        cameFromNode = null;
    }
}