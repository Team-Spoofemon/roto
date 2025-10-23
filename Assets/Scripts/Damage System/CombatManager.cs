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

    public void SingleAttack(HealthManager target, float damage)
    {
        target.TakeDamage(damage);
        new KnockbackEffect
        {
            strength = 2f,
            upwards = 0f,
            forceMode = ForceMode.Impulse,
        }.Apply(target);
    }

    public void SingleAttack(HealthManager target, float damage, KnockbackEffect knockback)
    {
        target.TakeDamage(damage);
        knockback.Apply(target);
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
