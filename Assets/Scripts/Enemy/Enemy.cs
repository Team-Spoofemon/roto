using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IHitHandler
{
    public EnemyMovement movement;
    public NavMeshAgent agent;
    //public int health = 100f;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageKnockback;

    [Header("Earth Giant Attack Setup")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Animator enemyAnimator;

    [SerializeField]
    private GameObject targetIndicatorPrefab;

    [SerializeField]
    private GameObject rootAttackPrefab;

    [SerializeField]
    private float attackRange = 4f;

    [SerializeField]
    private float attackCooldown = 2f;

    [SerializeField]
    private float telegraphDuration = 1f;

    [SerializeField]
    private float rootLifetime = 1f;

    private Coroutine followCoroutine;
    private bool isAttackSequenceRunning;
    private bool isOnCooldown;
    private bool canDamage;
    private Vector3 lockedTargetPosition;

    private void Update()
    {
        if (player == null || isAttackSequenceRunning || isOnCooldown)
            return;

        Vector3 enemyPos = agent != null ? agent.transform.position : transform.position;
        Vector3 playerPos = player.position;

        enemyPos.y = 0f;
        playerPos.y = 0f;

        float distanceToPlayer = Vector3.Distance(enemyPos, playerPos);
        Debug.Log("Earth Giant distance: " + distanceToPlayer);

        if (distanceToPlayer <= attackRange)
            StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        Debug.Log("AttackSequence START");
        isAttackSequenceRunning = true;
        isOnCooldown = true;

        movement.SetAttacking(true);
        lockedTargetPosition = player.position;

        GameObject indicator = null;

        if (targetIndicatorPrefab != null)
            indicator = Instantiate(targetIndicatorPrefab, lockedTargetPosition, Quaternion.identity);

        yield return new WaitForSeconds(telegraphDuration);

        if (indicator != null)
            Destroy(indicator);

        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");

        Vector3 attackDirection = lockedTargetPosition - transform.position;
        attackDirection.y = 0f;

        if (attackDirection.sqrMagnitude > 0.001f && rootAttackPrefab != null)
        {
            Quaternion rootRotation = Quaternion.LookRotation(attackDirection.normalized);
            GameObject rootAttack = Instantiate(rootAttackPrefab, lockedTargetPosition, rootRotation);

            HitBox hitBox = rootAttack.GetComponentInChildren<HitBox>();
            if (hitBox != null)
                hitBox.SetDamageSource(this);

            canDamage = true;
            yield return new WaitForSeconds(rootLifetime);
            canDamage = false;

            if (rootAttack != null)
                Destroy(rootAttack);
        }
        else
        {
            canDamage = false;
        }

        yield return new WaitForSeconds(attackCooldown);

        movement.SetAttacking(false);
        isAttackSequenceRunning = false;
        isOnCooldown = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        canDamage = false;
        isAttackSequenceRunning = false;
        isOnCooldown = false;

        if (agent != null)
            agent.enabled = false;
    }

    public void OnHit(HealthManager targetHealth)
    {
        if (!canDamage)
            return;

        CombatManager.Instance.SingleAttack(targetHealth, damage, transform, damageKnockback);
    }
}