using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IHitHandler
{
    public EnemyMovement movement;
    public NavMeshAgent agent;

    [SerializeField] private float damage;
    [SerializeField] private float damageKnockback;

    [Header("Earth Giant Attack Setup")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerShadowTarget;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private GameObject rootAttackPrefab;

    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float telegraphDuration = 1f;
    [SerializeField] private float rootLifetime = 1f;

    [SerializeField] private Collider playerCollider;

    private bool isAttackSequenceRunning;
    private bool isOnCooldown;
    private bool canDamage;
    private Vector3 lockedTargetPosition;

    private Coroutine attackCoroutine;
    private GameObject activeIndicator;
    private GameObject activeRootAttack;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (playerCollider == null && player != null)
            playerCollider = player.GetComponentInChildren<Collider>();

        if (playerShadowTarget == null && player != null)
        {
            Transform shadow = player.Find("ShadowTarget");
            if (shadow != null)
                playerShadowTarget = shadow;
        }
    }

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
            attackCoroutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        Debug.Log("AttackSequence START");
        isAttackSequenceRunning = true;
        isOnCooldown = true;

        if (movement != null)
            movement.SetAttacking(true);

        if (playerShadowTarget != null)
            lockedTargetPosition = playerShadowTarget.position;
        else if (player != null)
            lockedTargetPosition = player.position;
        else if (playerCollider != null)
            lockedTargetPosition = playerCollider.bounds.center;

        activeIndicator = null;

        if (targetIndicatorPrefab != null)
            activeIndicator = Instantiate(targetIndicatorPrefab, lockedTargetPosition, Quaternion.identity);

        yield return new WaitForSeconds(telegraphDuration);

        if (activeIndicator != null)
        {
            Destroy(activeIndicator);
            activeIndicator = null;
        }

        Vector3 attackDirection = lockedTargetPosition - transform.position;
        attackDirection.y = 0f;

        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");

        if (attackDirection.sqrMagnitude > 0.001f && rootAttackPrefab != null)
        {
            Quaternion rootRotation = Quaternion.LookRotation(attackDirection.normalized);
            activeRootAttack = Instantiate(rootAttackPrefab, lockedTargetPosition, rootRotation);

            Animator spawnedRootsAnimator = activeRootAttack.GetComponentInChildren<Animator>();
            if (spawnedRootsAnimator != null)
                spawnedRootsAnimator.SetTrigger("Attack");

            Debug.Log("Spawned Root Attack Position: " + activeRootAttack.transform.position);

            HitBox hitBox = activeRootAttack.GetComponentInChildren<HitBox>();
            if (hitBox != null)
                hitBox.SetDamageSource(this);

            canDamage = true;
            yield return new WaitForSeconds(rootLifetime);
            canDamage = false;

            if (activeRootAttack != null)
            {
                Destroy(activeRootAttack);
                activeRootAttack = null;
            }
        }
        else
        {
            canDamage = false;
        }

        yield return new WaitForSeconds(attackCooldown);

        if (movement != null)
            movement.SetAttacking(false);

        attackCoroutine = null;
        isAttackSequenceRunning = false;
        isOnCooldown = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (activeIndicator != null)
        {
            Destroy(activeIndicator);
            activeIndicator = null;
        }

        if (activeRootAttack != null)
        {
            Destroy(activeRootAttack);
            activeRootAttack = null;
        }

        canDamage = false;
        isAttackSequenceRunning = false;
        isOnCooldown = false;

        if (movement != null)
            movement.SetAttacking(false);

        if (agent != null && agent.gameObject != null)
            agent.enabled = false;
    }

    public void OnHit(HealthManager targetHealth)
    {
        if (!canDamage)
            return;

        CombatManager.Instance.SingleAttack(targetHealth, damage, transform, damageKnockback);
    }

    public void SetTarget(Transform target, Collider targetCollider)
    {
        player = target;
        playerCollider = targetCollider;

        if (player != null)
        {
            Transform shadow = player.Find("ShadowTarget");
            if (shadow != null)
                playerShadowTarget = shadow;
        }

        if (movement != null)
            movement.player = player;
    }
}