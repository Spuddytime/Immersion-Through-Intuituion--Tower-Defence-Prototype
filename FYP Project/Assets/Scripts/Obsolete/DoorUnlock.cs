using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    public GameObject lockpick; // Reference to the lockpick
    public float unlockDistance = 2f; // Distance required to unlock
    private bool isUnlocked = false;

    public Collider blockingCollider; // Physical collider to block the player
    public Collider triggerCollider;  // Trigger collider for interaction

    void Update()
    {
        if (lockpick != null && !isUnlocked)
        {
            float distance = Vector3.Distance(lockpick.transform.position, transform.position);
            
            // Check if the lockpick is close enough
            if (distance <= unlockDistance)
            {
                Debug.Log("Door Unlocked!");
                UnlockDoor();
            }
        }
    }

    void UnlockDoor()
    {
        isUnlocked = true;

        transform.position += new Vector3(0, 3, 0);

        // Disable the blocking collider to allow passage
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }
}
