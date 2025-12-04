using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IHitHandler
{
    public EnemyMovement movement;
    public NavMeshAgent agent;
    //public int health = 100;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageKnockback;

    private Coroutine followCoroutine;

    public override void OnDisable()
    {
        base.OnDisable();

        agent.enabled = false;
    }

    public void OnHit(HealthManager targetHealth)
    {
        CombatManager.Instance.SingleAttack(targetHealth, damage, transform, damageKnockback);
    }
}
