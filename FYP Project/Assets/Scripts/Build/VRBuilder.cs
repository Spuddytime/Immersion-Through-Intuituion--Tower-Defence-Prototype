using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// VR-style builder using a world-space ray and visible laser
public class VRBuilder : MonoBehaviour
{
    [Header("Ray")]
    public Transform rayOrigin;
    public float rayDistance = 50f;
    public LayerMask groundLayer;

    [Header("Build Data")]
    public BuildableOption[] buildOptions;
    public Transform cellHighlight;

    [Header("Highlight Materials")]
    public Material validHighlightMaterial;
    public Material invalidHighlightMaterial;

    [Header("Scene References")]
    public Transform startMarker;
    public Transform goalMarker;
    public PathTester pathTester;

    [Header("Laser Visuals")]
    public LineRenderer laserLine;
    public Transform hitMarker;

    private int currentBuildIndex = 0;

    private InputDevice rightHand;
    private bool lastTriggerState = false;

    void Start()
    {
        UpdateBuildModeUI();
        TryInitializeRightHand();
    }

    void Update()
    {
        // Reacquire controller if needed
        if (!rightHand.isValid)
        {
            TryInitializeRightHand();
        }

        HandleInput();
        UpdateRayVisuals();
    }

    void TryInitializeRightHand()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
            Debug.Log("Right hand controller found: " + rightHand.name);
        }
    }

    void HandleInput()
    {
        // VR trigger placement
        if (rightHand.isValid)
        {
            if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
            {
                if (triggerPressed && !lastTriggerState)
                {
                    TryPlace();
                }

                lastTriggerState = triggerPressed;
            }
        }

        // Optional desktop fallback for testing
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        // Optional remove test
        if (Input.GetMouseButtonDown(1))
        {
            TryRemove();
        }

        // Optional upgrade test
        if (Input.GetKeyDown(KeyCode.U))
        {
            TryUpgrade();
        }

        // Build mode selection
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SetBuildMode(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SetBuildMode(1);

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            SetBuildMode(2);

        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            SetBuildMode(3);
    }

    void SetBuildMode(int index)
    {
        if (buildOptions == null || index >= buildOptions.Length)
            return;

        currentBuildIndex = index;
        Debug.Log("VR Build Mode: " + buildOptions[index].name);
        UpdateBuildModeUI();
    }

    void UpdateBuildModeUI()
    {
        if (UIManager.Instance == null || buildOptions == null || buildOptions.Length == 0)
            return;

        UIManager.Instance.UpdateBuildMode(buildOptions[currentBuildIndex].name);
    }

    void UpdateRayVisuals()
    {
        if (rayOrigin == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        Vector3 startPos = rayOrigin.position + rayOrigin.forward * 0.05f;
        Vector3 rayEnd = rayOrigin.position + rayOrigin.forward * rayDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            rayEnd = hit.point;

            if (hitMarker != null)
            {
                hitMarker.position = hit.point + Vector3.up * 0.02f;
                hitMarker.gameObject.SetActive(true);
            }

            if (GridManager.Instance != null && cellHighlight != null)
            {
                if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
                {
                    Vector3 cellWorldPos = GridManager.Instance.GetWorldPosition(x, y);
                    cellHighlight.position = cellWorldPos + new Vector3(0f, 0.05f, 0f);
                    cellHighlight.gameObject.SetActive(true);

                    if (buildOptions != null && buildOptions.Length > 0)
                    {
                        BuildableOption option = buildOptions[currentBuildIndex];
                        bool isValid = IsValidPlacement(x, y, option);

                        Renderer rend = cellHighlight.GetComponentInChildren<Renderer>();
                        if (rend != null)
                        {
                            if (isValid && validHighlightMaterial != null)
                            {
                                rend.material = validHighlightMaterial;
                            }
                            else if (!isValid && invalidHighlightMaterial != null)
                            {
                                rend.material = invalidHighlightMaterial;
                            }
                        }
                    }
                }
                else
                {
                    cellHighlight.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);

            if (cellHighlight != null)
                cellHighlight.gameObject.SetActive(false);
        }

        if (laserLine != null)
        {
            laserLine.positionCount = 2;
            laserLine.SetPosition(0, startPos);
            laserLine.SetPosition(1, rayEnd);
        }
    }

    bool IsValidPlacement(int x, int y, BuildableOption option)
    {
        if (GridManager.Instance == null || option == null)
            return false;

        switch (option.type)
        {
            case BuildType.Wall:
                if (IsSpecialCell(x, y))
                    return false;

                if (GridManager.Instance.HasWall(x, y))
                    return false;

                if (WouldBlockPath(x, y))
                    return false;

                return true;

            case BuildType.Turret:
                if (!GridManager.Instance.HasWall(x, y))
                    return false;

                if (GridManager.Instance.HasTurret(x, y))
                    return false;

                return true;

            case BuildType.Trap:
                if (IsSpecialCell(x, y))
                    return false;

                if (GridManager.Instance.HasWall(x, y))
                    return false;

                if (GridManager.Instance.HasTrap(x, y))
                    return false;

                return true;
        }

        return false;
    }

    void TryPlace()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        if (buildOptions == null || buildOptions.Length == 0)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
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
                        placedSuccessfully = TryPlaceWall(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Turret:
                        placedSuccessfully = TryPlaceTurret(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Trap:
                        placedSuccessfully = TryPlaceTrap(x, y, option.prefab, option.cost);
                        break;
                }

                if (placedSuccessfully && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendMoney(option.cost);
                }
            }
        }
    }

    bool TryPlaceWall(int x, int y, GameObject prefab, int cost)
    {
        if (prefab == null)
            return false;

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot build on start or goal cell.");
            return false;
        }

        if (GridManager.Instance.HasWall(x, y))
            return false;

        if (WouldBlockPath(x, y))
        {
            Debug.Log("Cannot place wall here - it would block all paths.");
            return false;
        }

        bool placed = GridManager.Instance.PlaceWall(x, y, prefab, cost);

        if (placed && pathTester != null)
        {
            pathTester.TestPath();
        }

        return placed;
    }

    bool TryPlaceTurret(int x, int y, GameObject prefab, int cost)
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

        return GridManager.Instance.PlaceTurret(x, y, prefab, cost);
    }

    bool TryPlaceTrap(int x, int y, GameObject prefab, int cost)
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

        return GridManager.Instance.PlaceTrap(x, y, prefab, cost);
    }

    void TryRemove()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                int refund = GridManager.Instance.ClearCell(x, y);

                if (refund > 0 && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddMoney(refund);
                    Debug.Log("Refunded: " + refund);
                }

                if (pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }

    void TryUpgrade()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                bool upgraded = GridManager.Instance.TryUpgradeAtCell(x, y);

                if (upgraded)
                {
                    Debug.Log("Upgraded build at cell: " + x + ", " + y);
                }
            }
        }
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
}