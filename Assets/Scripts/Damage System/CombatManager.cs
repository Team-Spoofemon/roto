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
