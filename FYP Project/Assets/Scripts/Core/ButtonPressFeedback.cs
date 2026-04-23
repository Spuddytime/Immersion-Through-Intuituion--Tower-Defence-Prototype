using System.Collections;
using UnityEngine;

// Simple visual feedback for a pressable VR button
public class ButtonPressFeedback : MonoBehaviour
{
    public Transform buttonVisual;
    public Vector3 pressedOffset = new Vector3(0f, 0f, -0.03f);
    public float pressDuration = 0.08f;

    public Renderer targetRenderer;
    public Material normalMaterial;
    public Material pressedMaterial;

    private Vector3 startLocalPos;
    private bool isAnimating = false;

    void Start()
    {
        if (buttonVisual == null)
            buttonVisual = transform;

        startLocalPos = buttonVisual.localPosition;

        Debug.Log("ButtonPressFeedback Start on " + gameObject.name +
                  " | buttonVisual = " + buttonVisual.name +
                  " | startLocalPos = " + startLocalPos);

        if (targetRenderer != null && normalMaterial != null)
        {
            targetRenderer.material = normalMaterial;
        }
    }

   public void Press()
{

    if (targetRenderer != null && pressedMaterial != null)
    {
        targetRenderer.material = pressedMaterial;
    }

    if (!isAnimating)
    {
        StartCoroutine(AnimatePress());
    }
}

    IEnumerator AnimatePress()
    {
        isAnimating = true;

        if (targetRenderer != null && pressedMaterial != null)
        {
            targetRenderer.material = pressedMaterial;
        }

        Vector3 pressedPos = startLocalPos + pressedOffset;
        Debug.Log("Animating button from " + startLocalPos + " to " + pressedPos);

        float timer = 0f;
        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            float t = timer / pressDuration;
            buttonVisual.localPosition = Vector3.Lerp(startLocalPos, pressedPos, t);
            yield return null;
        }

        timer = 0f;
        while (timer < pressDuration)
        {
            timer += Time.deltaTime;
            float t = timer / pressDuration;
            buttonVisual.localPosition = Vector3.Lerp(pressedPos, startLocalPos, t);
            yield return null;
        }

        buttonVisual.localPosition = startLocalPos;

        if (targetRenderer != null && normalMaterial != null)
        {
            targetRenderer.material = normalMaterial;
        }

        isAnimating = false;
    }
}