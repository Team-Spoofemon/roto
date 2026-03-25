using UnityEngine;

public class SwingingChandelier : MonoBehaviour
{
    public float length = 3f;
    public float gravity = 9.81f;
    [Range(0f, 2f)]
    public float damping = 0.2f;
    public float maxAngle = 45f;

    public float startAngle = 25f;
    public float startAngularVelocity = 0f;

    public Vector3 localSwingAxis = Vector3.forward;

    float angle;
    float angularVelocity;
    Quaternion initialRotation;

    void Awake()
    {
        initialRotation = transform.localRotation;
        angle = Mathf.Clamp(startAngle, -maxAngle, maxAngle);
        angularVelocity = startAngularVelocity;
        ApplyRotation();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        float thetaRad = angle * Mathf.Deg2Rad;
        float angularVelRad = angularVelocity * Mathf.Deg2Rad;

        float angularAccelRad =
            -(gravity / length) * Mathf.Sin(thetaRad)
            - damping * angularVelRad;

        angularVelRad += angularAccelRad * dt;
        thetaRad += angularVelRad * dt;

        angle = thetaRad * Mathf.Rad2Deg;
        angularVelocity = angularVelRad * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.localRotation =
            initialRotation *
            Quaternion.AngleAxis(angle, localSwingAxis.normalized);
    }

    public void AddImpulse(float impulse)
    {
        angularVelocity += impulse;
    }

    public void ResetSwing()
    {
        angle = Mathf.Clamp(startAngle, -maxAngle, maxAngle);
        angularVelocity = 0f;
        ApplyRotation();
    }
}