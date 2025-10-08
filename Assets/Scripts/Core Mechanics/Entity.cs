using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable, IDamages
{
    public int health;
    public int attackDamage;

    public void TakesDamage(int damage)
    {
        health -= damage;
    }

    public void DealDamage(GameObject target)
    {
        var targetStats = target.GetComponent<Entity>();
        if (targetStats != null)
        {
            targetStats.TakesDamage(attackDamage);
        }
        if (targetStats.health <= 0)
        {
            targetStats.Die();
        }
    }

    public virtual void Attack(Entity target) { }

    public virtual void Die() { }
}
