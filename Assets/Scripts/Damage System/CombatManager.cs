using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("CombatManager duplicate created!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("CombatManager created!");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Method to deal with attack animations (lockstate on/off)
    public IEnumerator PlayAttackAndLock(Collider col, Animator anim, string triggerName)
    {
        col.enabled = false;

        anim.ResetTrigger(triggerName);
        anim.SetTrigger(triggerName);

        yield return null;

        var info = anim.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;

        col.enabled = true;
        yield return new WaitForSeconds(duration);
        col.enabled = false;
    }

    // Deal knockback to entity with force
    public void KnockbackEntity(Rigidbody rb, Transform executionSource, float force)
    {
        if (!rb || !executionSource)
            return;

        // Keep the body upright
        rb.constraints |=
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Horizontal-only push away from attacker
        Vector3 dir = rb.worldCenterOfMass - executionSource.position;
        dir.y = 0f;

        float sqr = dir.sqrMagnitude;
        if (sqr < 1e-6f)
            return;
        dir /= Mathf.Sqrt(sqr); // normalize

        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    // Default attack type (one shot hit)
    public void SingleAttack(HealthManager target, float damage)
    {
        target.TakeDamage(damage);
    }

    // Default attack type (one shot hit) with knockback force
    public void SingleAttack(
        HealthManager target,
        float damage,
        Transform attackerLocation,
        float force
    )
    {
        target.TakeDamage(damage);

        var targetRb = target.GetComponentInParent<Rigidbody>();
        if (targetRb != null)
            KnockbackEntity(targetRb, attackerLocation, force);
    }

    public void DOTAttack(HealthManager target, float damage, float sec, DOTType dot)
    {
        // implement later?
    }

    public void ProjectileAttack(HealthManager target, Projectile projectile) { }

    public void AreaAttack(HealthManager target, float damage, float radius)
    {
        // implement later?
    }
}
