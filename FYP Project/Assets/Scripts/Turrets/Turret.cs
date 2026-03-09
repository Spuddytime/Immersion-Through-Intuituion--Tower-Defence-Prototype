using UnityEngine;

// Simple turret that finds a valid target, rotates toward it, and deals instant damage
public class Turret : MonoBehaviour
{
    [Header("Stats")]
    public float range = 4f;
    public float fireRate = 1f;
    public int damage = 1;
    public TurretTargetType targetType = TurretTargetType.GroundOnly;

    [Header("Rotation")]
    public Transform rotatingHead; // Leave empty to rotate the whole turret
    public float turnSpeed = 8f;

    [Tooltip("Used if the turret model faces the wrong direction")]
    public Vector3 modelRotationOffset = new Vector3(0f, 180f, 0f);

    private float fireCooldown = 0f;
    private EnemyHealth currentTarget;

    void Update()
    {
        FindTarget();

        if (currentTarget != null)
        {
            RotateTowardsTarget();

            fireCooldown -= Time.deltaTime;

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            fireCooldown = Mathf.Max(fireCooldown - Time.deltaTime, 0f);
        }
    }

    void FindTarget()
    {
        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        EnemyHealth closestValidTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy == null)
                continue;

            if (!CanTarget(enemy))
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= range && distance < closestDistance)
            {
                closestDistance = distance;
                closestValidTarget = enemy;
            }
        }

        currentTarget = closestValidTarget;
    }

    bool CanTarget(EnemyHealth enemy)
    {
        switch (targetType)
        {
            case TurretTargetType.GroundOnly:
                return enemy.unitType == EnemyUnitType.Ground;

            case TurretTargetType.FlyingOnly:
                return enemy.unitType == EnemyUnitType.Flying;

            case TurretTargetType.Both:
                return true;
        }

        return false;
    }

    void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;

        Transform partToRotate = rotatingHead != null ? rotatingHead : transform;

        Vector3 direction = currentTarget.transform.position - partToRotate.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            Quaternion rotationOffset = Quaternion.Euler(modelRotationOffset);

            partToRotate.rotation = Quaternion.Slerp(
                partToRotate.rotation,
                targetRotation * rotationOffset,
                turnSpeed * Time.deltaTime
            );
        }
    }

    void Fire()
    {
        if (currentTarget == null)
            return;

        currentTarget.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}