using UnityEngine;

public struct Projectile
{
    public float damage { get; }
    public float speed { get; }
    public float knockback_Force { get; }
    public float knockback_jumpFactor { get; }
    public DOTType dot { get; }
}
