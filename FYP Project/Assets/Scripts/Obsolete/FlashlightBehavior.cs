using UnityEngine;

public class FlashlightBehavior : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Vector3 offset = new Vector3(0, 0, 1); // Position offset from the player
    public float followDistance = 2f; // Maximum distance before it starts following
    public float followSpeed = 5f; // Speed at which the flashlight follows the player

    private Rigidbody rb; // Reference to the flashlight's Rigidbody

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned!");
        }
    }

    private void Update()
    {
        if (player != null)
        {
            // Check the distance between the flashlight and the player
            float distance = Vector3.Distance(transform.position, player.position);

            // If the flashlight is too far, start following the player smoothly
            if (distance > followDistance)
            {
                Debug.Log("Flashlight is too far! Moving closer to the player.");
                SmoothFollowPlayer();
            }
        }
    }

    private void SmoothFollowPlayer()
    {
        // Temporarily disable Rigidbody physics while moving
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Smoothly move the flashlight towards the target position
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Re-enable Rigidbody physics after reaching the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f && rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
