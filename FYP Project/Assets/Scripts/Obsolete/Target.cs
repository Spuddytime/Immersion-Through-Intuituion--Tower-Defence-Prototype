using UnityEngine;

public class TargetBehavior : MonoBehaviour
{
    public GameObject door; // Reference to the door to unlock

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a projectile
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Target Hit!");
            UnlockDoor(); // Call the method to unlock the door
            Destroy(gameObject); // Destroy the target after being hit
        }
    }

    void UnlockDoor()
    {
        if (door != null)
        {
            // Example: Move the door upward to "open" it
            door.transform.position += new Vector3(0, 3, 0);
        }
    }
}
