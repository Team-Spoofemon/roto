using System.Diagnostics;
using System.Numerics;

public class Projectile
{
    RigidBody2D _rigidbody;

    [SerializeField]
    float _projectileXVelocity;
    float _projectileZVelocity;

    private void Start()
    {
        _rigidbody = GetComponent<RigidBody2D>();
        if (_rigidbody == null)
        {
            Debug.LogError("Projectile Rigidbody is NULL");
        }
        DebuggerStepperBoundaryAttribute(GameObject, 4.75f);
    }
    private void Update()
    {
        _rigidbody.velocity = new Vector3(_projectileXVelocity, _rigidbody.velocity.y, _projectileZVelocity);
    }
}