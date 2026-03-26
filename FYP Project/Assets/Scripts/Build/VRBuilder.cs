using UnityEngine;

// Oh Ma LAWD he coming
public class VRBuilder : MonoBehaviour
{
    public Transform rayOrigin; // camera or controller later
    public float rayDistance = 50f;

    public LayerMask groundLayer;
    public Transform cellHighlight;

    public BuildableOption[] buildOptions;
    int currentBuildIndex = 0;

    void Update()
    {
        HandleInput();
        UpdateRay();
    }

    void HandleInput()
    {
        // simulate trigger with mouse click
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        // switch build types
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentBuildIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentBuildIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentBuildIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentBuildIndex = 3;
    }

    void UpdateRay()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                Vector3 pos = GridManager.Instance.GetWorldPosition(x, y);
                cellHighlight.position = pos + Vector3.up * 0.05f;
                cellHighlight.gameObject.SetActive(true);
                return;
            }
        }

        cellHighlight.gameObject.SetActive(false);
    }

    void TryPlace()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                BuildableOption option = buildOptions[currentBuildIndex];

                switch (option.type)
                {
                    case BuildType.Wall:
                        GridManager.Instance.PlaceWall(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Turret:
                        GridManager.Instance.PlaceTurret(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Trap:
                        GridManager.Instance.PlaceTrap(x, y, option.prefab, option.cost);
                        break;
                }
            }
        }
    }
}