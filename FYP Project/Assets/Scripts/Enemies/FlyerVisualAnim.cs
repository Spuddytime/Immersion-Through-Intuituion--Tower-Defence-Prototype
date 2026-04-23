using UnityEngine;

public class FlyerVisualAnim : MonoBehaviour
{
    public Transform visualRoot;
    public Transform leftWing;
    public Transform rightWing;

    [Header("Hover")]
    public float hoverAmount = 0.08f;
    public float hoverSpeed = 3f;

    [Header("Tilt")]
    public float tiltAmount = 6f;
    public float tiltSpeed = 2f;

    [Header("Wing Flap")]
    public bool useWingFlap = true;
    public float flapAngle = 20f;
    public float flapSpeed = 8f;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    void Start()
    {
        if (visualRoot == null)
            visualRoot = transform;

        startLocalPos = visualRoot.localPosition;
        startLocalRot = visualRoot.localRotation;
    }

    void Update()
    {
        float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
        float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;

        visualRoot.localPosition = startLocalPos + new Vector3(0f, hover, 0f);
        visualRoot.localRotation = startLocalRot * Quaternion.Euler(tilt, 0f, 0f);

        if (useWingFlap)
        {
            float flap = Mathf.Sin(Time.time * flapSpeed) * flapAngle;

            if (leftWing != null)
                leftWing.localRotation = Quaternion.Euler(0f, 0f, flap);

            if (rightWing != null)
                rightWing.localRotation = Quaternion.Euler(0f, 0f, -flap);
        }
    }
}