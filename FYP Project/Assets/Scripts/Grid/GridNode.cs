using UnityEngine;

// Represents a single cell in the grid
public class GridNode
{
    public int x;
    public int y;

    public bool isBlocked;

    public Vector3 worldPosition;

    public GameObject wallObject;
    public GameObject turretObject;
    public GameObject trapObject;

    public int wallCost;
    public int turretCost;
    public int trapCost;

    public GridNode cameFromNode;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;

        isBlocked = false;

        wallObject = null;
        turretObject = null;
        trapObject = null;

        //adding a cost to the grid so sell function can work correctly

        wallCost = 0;
        turretCost = 0;
        trapCost = 0;

        cameFromNode = null;
    }
}