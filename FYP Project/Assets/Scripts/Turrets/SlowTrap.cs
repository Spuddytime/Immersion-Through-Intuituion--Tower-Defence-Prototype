using System.Collections;
using UnityEngine;

// Persistent trap with cooldown + smooth visual feedback
public class SlowTrap : MonoBehaviour
{
    [Header("Slow Effect")]
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1.5f;

    [Header("Cooldown")]
    public float cooldownDuration = 3f;
    private bool trapReady = true;

    [Header("Visuals")]
    public Renderer trapRenderer;
    public Color readyColor = Color.cyan;
    public Color cooldownColor = Color.red;
    public float emissionIntensity = 2f;

    private Material trapMaterial;

    void Start()
    {
        if (trapRenderer != null)
        {
            trapMaterial = trapRenderer.material;
        }

        UpdateVisualInstant(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!trapReady)
            return;

        EnemyMover enemy = other.GetComponent<EnemyMover>();

        if (enemy != null)
        {
            enemy.ApplySlow(slowMultiplier, slowDuration);
            StartCoroutine(CooldownRoutine());
        }
    }

    IEnumerator CooldownRoutine()
    {
        trapReady = false;

        float timer = 0f;

        while (timer < cooldownDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cooldownDuration;

            UpdateVisualLerp(t);

            yield return null;
        }

        trapReady = true;
        UpdateVisualInstant(true);
    }

    void UpdateVisualInstant(bool ready)
    {
        if (trapMaterial == null)
            return;

        Color color = ready ? readyColor : cooldownColor;

        trapMaterial.color = color;
        trapMaterial.SetColor("_EmissionColor", color * emissionIntensity);
    }

    void UpdateVisualLerp(float t)
    {
        if (trapMaterial == null)
            return;

        // cooldown → ready (red → cyan)
        Color color = Color.Lerp(cooldownColor, readyColor, t);

        trapMaterial.color = color;
        trapMaterial.SetColor("_EmissionColor", color * emissionIntensity);
    }
}