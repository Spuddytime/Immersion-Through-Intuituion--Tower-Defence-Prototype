using System.Collections;
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

    [Header("Placement Feedback")]
    public bool usePlacementPop = true;
    public float popDuration = 0.15f;
    public float popStartScaleMultiplier = 0.2f;

    [Header("Haptics")]
    public bool useHaptics = true;
    public float placeHapticStrength = 0.4f;
    public float placeHapticDuration = 0.08f;
    public float removeHapticStrength = 0.25f;
    public float removeHapticDuration = 0.06f;
    public float upgradeHapticStrength = 0.5f;
    public float upgradeHapticDuration = 0.1f;

    private int currentBuildIndex = 0;

    private InputDevice rightHand;

    private bool lastTriggerState = false;
    private bool lastGripState = false;
    private bool lastPrimaryState = false;
    private bool lastSecondaryState = false;

    void Start()
    {
        UpdateBuildModeUI();
        TryInitializeRightHand();
    }

    void Update()
    {
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
            Debug.Log("Right controller connected: " + rightHand.name);
        }
    }

    void HandleInput()
    {
        if (!rightHand.isValid)
            return;

        // Trigger = Place
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
        {
            if (triggerPressed && !lastTriggerState)
            {
                TryPlace();
            }
            lastTriggerState = triggerPressed;
        }

        // Grip = Remove
        if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed))
        {
            if (gripPressed && !lastGripState)
            {
                TryRemove();
            }
            lastGripState = gripPressed;
        }

        // Primary button (A on Quest right controller) = Cycle build mode
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed))
        {
            if (primaryPressed && !lastPrimaryState)
            {
                CycleBuildMode();
            }
            lastPrimaryState = primaryPressed;
        }

        // Secondary button (B on Quest right controller) = Upgrade
        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPressed))
        {
            if (secondaryPressed && !lastSecondaryState)
            {
                TryUpgrade();
            }
            lastSecondaryState = secondaryPressed;
        }
    }

    void CycleBuildMode()
    {
        if (buildOptions == null || buildOptions.Length == 0)
            return;

        currentBuildIndex++;

        if (currentBuildIndex >= buildOptions.Length)
            currentBuildIndex = 0;

        Debug.Log("VR Build Mode: " + buildOptions[currentBuildIndex].name);
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

    void SendHaptics(float amplitude, float duration)
    {
        if (!useHaptics)
            return;

        if (!rightHand.isValid)
            return;

        rightHand.SendHapticImpulse(0u, amplitude, duration);
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
                GameObject placedObject = null;

                switch (option.type)
                {
                    case BuildType.Wall:
                        placedSuccessfully = TryPlaceWall(x, y, option.prefab, option.cost, out placedObject);
                        break;

                    case BuildType.Turret:
                        placedSuccessfully = TryPlaceTurret(x, y, option.prefab, option.cost, out placedObject);
                        break;

                    case BuildType.Trap:
                        placedSuccessfully = TryPlaceTrap(x, y, option.prefab, option.cost, out placedObject);
                        break;
                }

                if (placedSuccessfully && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendMoney(option.cost);

                    if (usePlacementPop && placedObject != null)
                    {
                        StartCoroutine(PlayPlacementPop(placedObject.transform));
                    }

                    SendHaptics(placeHapticStrength, placeHapticDuration);
                }
            }
        }
    }

    bool TryPlaceWall(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

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

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.wallObject;

            if (pathTester != null)
            {
                pathTester.TestPath();
            }
        }

        return placed;
    }

    bool TryPlaceTurret(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

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

        bool placed = GridManager.Instance.PlaceTurret(x, y, prefab, cost);

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.turretObject;
        }

        return placed;
    }

    bool TryPlaceTrap(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

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

        bool placed = GridManager.Instance.PlaceTrap(x, y, prefab, cost);

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.trapObject;
        }

        return placed;
    }

    IEnumerator PlayPlacementPop(Transform placedTransform)
    {
        if (placedTransform == null)
            yield break;

        Vector3 finalScale = placedTransform.localScale;
        Vector3 startScale = finalScale * popStartScaleMultiplier;

        placedTransform.localScale = startScale;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            placedTransform.localScale = Vector3.Lerp(startScale, finalScale, t);
            yield return null;
        }

        placedTransform.localScale = finalScale;
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
                    SendHaptics(removeHapticStrength, removeHapticDuration);
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
                    SendHaptics(upgradeHapticStrength, upgradeHapticDuration);
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

        List<GridNode> testPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        GridManager.Instance.SetCellBlocked(x, y, false);

        return testPath == null;
    }
}