using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KnockbackEffect
{
    public float strength = 2f; // how hard to push
    public float upwards = 0f; // optional lift
    public ForceMode forceMode = ForceMode.Impulse;

    // Per-target last hit direction (set at the moment of collision)
    private static readonly Dictionary<int, Vector3> _lastHitDir = new();

    /// <summary>
    /// Call this at the moment of impact BEFORE you call CombatManager.SingleAttack(...).
    /// Direction should be the actual attack direction (e.g., swing velocity, projectile velocity,
    /// or contact-point vector from attacker to target).
    /// </summary>
    public static void RecordHit(DamageManager target, Vector3 direction)
    {
        if (target == null)
            return;
        if (direction.sqrMagnitude < 1e-6f)
            return;
        _lastHitDir[target.GetInstanceID()] = direction.normalized;
    }

    /// <summary>
    /// Applies knockback to the target using the most recently recorded hit direction.
    /// If no direction was recorded for this target, does nothing.
    /// </summary>
    public void Apply(DamageManager target)
    {
        if (target == null)
            return;

        // Pull and consume the stored direction
        int id = target.GetInstanceID();
        if (!_lastHitDir.TryGetValue(id, out var dir))
            return;
        _lastHitDir.Remove(id);

        var rb = target.GetComponent<Rigidbody>();
        if (rb == null)
            return; // requires a Rigidbody per your setup (non-kinematic)

        if (upwards != 0f)
            dir = (dir + Vector3.up * upwards).normalized;

        float s = Mathf.Max(0f, strength);
        if (s <= 0f)
            return;

        rb.AddForce(dir * s, forceMode);
    }
}
