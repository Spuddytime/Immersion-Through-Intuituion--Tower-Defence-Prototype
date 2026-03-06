using UnityEngine;

// Represents a single cell in the grid
public class GridNode
{
    // Grid coordinates
    public int x;
    public int y;

    // Whether this cell is currently blocked by an object (wall, tower, etc.)
    public bool isBlocked;

    // World position of the centre of the grid cell
    public Vector3 worldPosition;

    // Reference to the object placed in this cell
    public GameObject placedObject;

    // Constructor used when creating the grid
    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;

        // By default cells start empty
        isBlocked = false;
        placedObject = null;
    }
}