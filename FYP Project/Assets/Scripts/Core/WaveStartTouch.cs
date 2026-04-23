using UnityEngine;

public class WaveStartTouch : MonoBehaviour
{
    public WaveSpawner waveSpawner;
    public ButtonPressFeedback pressFeedback;
    public string controllerTag = "XRController";

    private bool isArmed = true;

    private void OnTriggerEnter(Collider other)
    {
        // Check it's the VR controller
        if (!other.CompareTag(controllerTag))
        {
            Debug.Log("Ignored collision with: " + other.name);
            return;
        }

        Debug.Log("Controller touched button: " + other.name);

        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner is NOT assigned!");
            return;
        }

        if (!isArmed)
        {
            Debug.Log("Button not armed yet.");
            return;
        }

        if (waveSpawner.IsSpawning)
        {
            Debug.Log("Wave already spawning.");
            return;
        }

        
        if (pressFeedback != null)
        {
            Debug.Log("Calling PressFeedback");
            pressFeedback.Press();
        }
        else
        {
            Debug.LogError("PressFeedback is NULL!");
        }

        Debug.Log("Starting next wave...");
        waveSpawner.StartNextWave();

        isArmed = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(controllerTag))
            return;

        Debug.Log("Controller left button");

        isArmed = true;
    }
}