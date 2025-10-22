using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private static CombatManager instance;
    public static CombatManager Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get
        {
            return instance
                ?? (instance = new GameObject("CombatManager").AddComponent<CombatManager>());
        }
    }

    public void SingleAttack(DamageManager target, float damage)
    {
        Debug.Log("SINGLE ATTACK");
        target.TakeDamage(damage);
        new KnockbackEffect
        {
            strength = 2f,
            upwards = 0f,
            forceMode = ForceMode.Impulse,
        }.Apply(target);
    }

    public void SingleAttack(DamageManager target, float damage, KnockbackEffect knockback)
    {
        target.TakeDamage(damage);
        knockback.Apply(target);
    }

    public void DOTAttack(DamageManager target, float damage, float sec, DOTType dot)
    {
        // implement later?
    }

    public void ProjectileAttack(DamageManager target, Projectile projectile) { }

    public void AreaAttack(DamageManager target, float damage, float radius)
    {
        // implement later?
    }
}
