using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Moves an enemy along a path and recalculates if the maze changes
public class EnemyMover : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public Transform goalMarker;
    public int damageToBase = 1;

    [Header("Height")]
    public float groundHeightOffset = 0.5f;
    public float flyingHeightOffset = 1.5f;
    public EnemyUnitType unitType = EnemyUnitType.Ground;

    [Header("Visuals")]
    public Transform visualRoot;
    public Vector3 visualRotationOffset = Vector3.zero;

    [Header("Hover")]
    public bool useHover = false;
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 3f;

    [Header("Slow Visuals")]
    public Color slowedColor = Color.cyan;

    private List<GridNode> path;
    private int currentPathIndex = 0;
    private BaseHealth baseHealth;

    private Vector3 visualBaseLocalPosition;

    private float speedMultiplier = 1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    private void OnEnable()
    {
        PathTester.OnPathUpdated += RecalculatePath;
    }

    private void OnDisable()
    {
        PathTester.OnPathUpdated -= RecalculatePath;
    }

    private void Start()
    {
        baseHealth = FindFirstObjectByType<BaseHealth>();

        if (visualRoot != null)
        {
            visualBaseLocalPosition = visualRoot.localPosition;
            visualRoot.localRotation = Quaternion.Euler(visualRotationOffset);
        }

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Make sure each enemy gets its own material instance
            renderers[i].material = new Material(renderers[i].material);

            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public void SetPath(List<GridNode> newPath)
    {
        path = newPath;
        currentPathIndex = 0;

        if (path != null && path.Count > 0)
        {
            transform.position = GetWorldPositionWithHeight(path[0]);
        }
    }

    void Update()
    {
        if (path == null || path.Count == 0)
            return;

        if (currentPathIndex >= path.Count)
            return;

        Vector3 targetPosition = GetWorldPositionWithHeight(path[currentPathIndex]);
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0f;

        // Rotate root toward movement direction
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Keep visual model in the correct local orientation
        if (visualRoot != null)
        {
            visualRoot.localRotation = Quaternion.Euler(visualRotationOffset);

            if (useHover)
            {
                Vector3 hoverPosition = visualBaseLocalPosition;
                hoverPosition.y += Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
                visualRoot.localPosition = hoverPosition;
            }
            else
            {
                visualRoot.localPosition = visualBaseLocalPosition;
            }
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * speedMultiplier * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex >= path.Count)
            {
                ReachGoal();
            }
        }
    }

    Vector3 GetWorldPositionWithHeight(GridNode node)
    {
        float heightOffset = groundHeightOffset;

        if (unitType == EnemyUnitType.Flying)
        {
            heightOffset = flyingHeightOffset;
        }

        return node.worldPosition + Vector3.up * heightOffset;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        StartCoroutine(SlowRoutine(multiplier, duration));
    }

    IEnumerator SlowRoutine(float multiplier, float duration)
    {
        speedMultiplier *= multiplier;

        // Apply slowed tint
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = slowedColor;
            }
        }

        yield return new WaitForSeconds(duration);

        speedMultiplier /= multiplier;

        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    void ReachGoal()
    {
        Debug.Log("Enemy reached the goal.");

        if (baseHealth != null)
        {
            baseHealth.TakeDamage(damageToBase);
        }

        Destroy(gameObject);
    }

    void RecalculatePath()
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null || goalMarker == null)
            return;

        if (!GridManager.Instance.GetXY(transform.position, out int currentX, out int currentY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        List<GridNode> newPath = Pathfinder.Instance.FindPath(currentX, currentY, goalX, goalY);

        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            currentPathIndex = 0;
        }
    }
}