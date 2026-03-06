using UnityEngine;

// Handles player input for placing and removing buildable objects using the mouse
public class MouseBuilder : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject wallPrefab;
    public Transform cellHighlight;
    public LayerMask groundLayer;

    public Transform startMarker;
    public Transform goalMarker;

    public PathTester pathTester;

    void Update()
    {
        UpdateHighlight();

        // Left click places a wall
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        // Right click removes a wall
        if (Input.GetMouseButtonDown(1))
        {
            TryRemove();
        }
    }

    // Moves the highlight object to the grid cell under the mouse
    void UpdateHighlight()
    {
        if (mainCamera == null || cellHighlight == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                Vector3 cellWorldPos = GridManager.Instance.GetWorldPosition(x, y);
                cellHighlight.position = cellWorldPos + new Vector3(0f, 0.05f, 0f);
                cellHighlight.gameObject.SetActive(true);
                return;
            }
        }

        cellHighlight.gameObject.SetActive(false);
    }

    // Attempts to place a wall where the mouse is pointing
    void TryPlace()
    {
        if (mainCamera == null || wallPrefab == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                // Prevent building on start or goal cells
                if (IsSpecialCell(x, y))
                {
                    Debug.Log("Cannot build on start or goal cell.");
                    return;
                }

                bool placed = GridManager.Instance.PlaceObject(x, y, wallPrefab);

                // If placement succeeded, update the path automatically
                if (placed && pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }

    // Checks whether the selected cell is the start or goal cell
    bool IsSpecialCell(int x, int y)
    {
        if (startMarker != null && GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
        {
            if (x == startX && y == startY)
                return true;
        }

        if (goalMarker != null && GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
        {
            if (x == goalX && y == goalY)
                return true;
        }

        return false;
    }

    // Attempts to remove a wall from the clicked cell
    void TryRemove()
    {
        if (mainCamera == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                GridManager.Instance.ClearCell(x, y);

                // Update the path after removing a wall
                if (pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }
}