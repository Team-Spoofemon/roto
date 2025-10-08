using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributesManager : MonoBehaviour
{
    public int health, attackDamage;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    public void DealDamage(GameObject target)
    {
        var ai = target.GetComponent<Entity>();
        if (ai == null)
        {
            Debug.Log("Target has no entity script. Add an enemy AI script.");
            return;
        }

        var atm = target.GetComponent<AttributesManager>();
        if (atm != null)
        {
            atm.TakeDamage(attackDamage);
        }
        if (atm.health <= 0)
        {
            ai.Die();
        }
    }
}
