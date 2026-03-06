using UnityEngine;

public class AddMeshCollidersToMaze : MonoBehaviour
{
    void Start()
    {
        // Loop through all child objects of the maze
        foreach (Transform child in transform)
        {
            // Check if the child already has a collider
            if (child.GetComponent<Collider>() == null)
            {
                // Add a Mesh Collider to the child
                MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();

                // Set the Mesh Collider to be static and more efficient
                meshCollider.convex = false; // Only works for static objects
            }
        }
    }
}
