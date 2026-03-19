using UnityEngine;

// Handles player input for placing buildable objects using the mouse
public class MouseBuilder : MonoBehaviour
{
    public Camera mainCamera;
    public BuildableOption[] buildOptions;
    public Transform cellHighlight;
    public LayerMask groundLayer;

    public Transform startMarker;
    public Transform goalMarker;

    public PathTester pathTester;

    int currentBuildIndex = 0;

    void Start()
    {
        UpdateBuildModeUI();
    }

    void Update()
    {
        HandleBuildModeInput();
        UpdateHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRemove();
        }
    }

    void HandleBuildModeInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetBuildMode(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetBuildMode(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetBuildMode(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetBuildMode(3);
    }

    void SetBuildMode(int index)
    {
        if (buildOptions == null || index >= buildOptions.Length)
            return;

        currentBuildIndex = index;

        Debug.Log("Build Mode: " + buildOptions[index].name);
        UpdateBuildModeUI();
    }

    void UpdateBuildModeUI()
    {
        if (UIManager.Instance == null || buildOptions == null || buildOptions.Length == 0)
            return;

        UIManager.Instance.UpdateBuildMode(buildOptions[currentBuildIndex].name);
    }

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

    void TryPlace()
    {
        if (mainCamera == null || GridManager.Instance == null)
            return;

        if (buildOptions == null || buildOptions.Length == 0)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                BuildableOption option = buildOptions[currentBuildIndex];

                if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(option.cost))
                {
                    Debug.Log("Not enough money for " + option.name);
                    return;
                }

                bool placedSuccessfully = false;

                switch (option.type)
                {
                    case BuildType.Wall:
                        placedSuccessfully = TryPlaceWall(x, y, option.prefab);
                        break;

                    case BuildType.Turret:
                        placedSuccessfully = TryPlaceTurret(x, y, option.prefab);
                        break;

                    case BuildType.Trap:
                        placedSuccessfully = TryPlaceTrap(x, y, option.prefab);
                        break;
                }

                if (placedSuccessfully && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendMoney(option.cost);
                }
            }
        }
    }

    bool TryPlaceWall(int x, int y, GameObject prefab)
    {
        if (prefab == null)
            return false;

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot build on start or goal cell.");
            return false;
        }

        if (GridManager.Instance.HasWall(x, y))
        {
            return false;
        }

        if (WouldBlockPath(x, y))
        {
            Debug.Log("Cannot place wall here - it would block all paths.");
            return false;
        }

        bool placed = GridManager.Instance.PlaceWall(x, y, prefab);

        if (placed && pathTester != null)
        {
            pathTester.TestPath();
        }

        return placed;
    }

    bool TryPlaceTurret(int x, int y, GameObject prefab)
    {
        if (prefab == null)
            return false;

        if (!GridManager.Instance.HasWall(x, y))
        {
            Debug.Log("Turrets must be placed on an existing wall.");
            return false;
        }

        if (GridManager.Instance.HasTurret(x, y))
        {
            Debug.Log("This wall already has a turret.");
            return false;
        }

        return GridManager.Instance.PlaceTurret(x, y, prefab);
    }

    bool TryPlaceTrap(int x, int y, GameObject prefab)
    {
        if (prefab == null)
            return false;

        if (GridManager.Instance.HasWall(x, y))
        {
            Debug.Log("Traps must be placed on open ground, not on walls.");
            return false;
        }

        if (GridManager.Instance.HasTrap(x, y))
        {
            Debug.Log("This tile already has a trap.");
            return false;
        }

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot place trap on start or goal cell.");
            return false;
        }

        return GridManager.Instance.PlaceTrap(x, y, prefab);
    }

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

    bool WouldBlockPath(int x, int y)
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null)
            return true;

        if (startMarker == null || !GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
            return true;

        if (goalMarker == null || !GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return true;

        GridManager.Instance.SetCellBlocked(x, y, true);

        var testPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        GridManager.Instance.SetCellBlocked(x, y, false);

        return testPath == null;
    }

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

                if (pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }
}