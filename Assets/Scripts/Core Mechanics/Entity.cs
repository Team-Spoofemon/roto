using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wait until meeting for more talk can be had about class design.

public abstract class Entity : MonoBehaviour, IDamageable, IDamages
{
    public int health;
    public int attackDamage;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }

    public virtual void DealDamage(GameObject target)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    public virtual void Attack(IDamageable target) { }

    public virtual void Die() { }
}
