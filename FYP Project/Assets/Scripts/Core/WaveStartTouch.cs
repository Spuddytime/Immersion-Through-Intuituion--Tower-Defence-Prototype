using UnityEngine;

public class WaveStartTouch : MonoBehaviour
{
    public WaveSpawner waveSpawner;
    public string controllerTag = "XRController";

    private bool isArmed = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(controllerTag))
            return;

        if (waveSpawner == null)
            return;

        if (!isArmed)
            return;

        if (waveSpawner.IsSpawning)
            return;

        Debug.Log($"Wave button pressed by {other.name}, using spawner {waveSpawner.gameObject.name} | ID: {waveSpawner.GetInstanceID()}");

        waveSpawner.StartNextWave();
        isArmed = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(controllerTag))
            return;

        isArmed = true;
    }
}