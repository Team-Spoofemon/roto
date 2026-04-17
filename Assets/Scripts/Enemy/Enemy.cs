using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IHitHandler
{
    public enum EnemyClass
    {
        EarthGiant,
        Earthling,
        GiantElite,
        Boss
    }

    public enum EnemyType
    {
        Tree,
        Stone,
        Ember,
        Water
    }

    public EnemyMovement movement;
    public NavMeshAgent agent;

    [SerializeField] private EnemyClass enemyClass;
    [SerializeField] private EnemyType enemyType;

    [SerializeField] private float damage;
    [SerializeField] private float damageKnockback;

    [Header("Target Setup")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerShadowTarget;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private Animator enemyAnimator;

    [Header("Attack Setup")]
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float telegraphDuration = 1f;
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private float eliteMeleeRange = 2f;
    [SerializeField] private float stoneArcHeight = 3f;
    [SerializeField] private float stoneTravelDuration = 0.8f;

    [Header("Tree Giant Attack")]
    [SerializeField] private GameObject rootAttackPrefab;
    [SerializeField] private float rootLifetime = 1f;

    [Header("Stone Giant Attack")]
    [SerializeField] private GameObject stoneProjectilePrefab;
    [SerializeField] private Transform stoneThrowPoint;
    [SerializeField] private float stoneThrowDelay = 0.3f;
    [SerializeField] private float rockslideProbability = 0.35f;
    [SerializeField] private float rockslideLock = 3f;
    private SpawnRockslide rockslideSpawner;

    private bool isAttackSequenceRunning;
    private bool isOnCooldown;
    private bool canDamage;
    private Vector3 lockedTargetPosition;

    private Coroutine attackCoroutine;
    private GameObject activeIndicator;
    private GameObject activeRootAttack;

    private static bool rockslideActive;

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
            Transform shadow = player.Find("Player Shadow");
            if (shadow != null)
                playerShadowTarget = shadow;
        }

        if (rockslideSpawner == null)
        {
            GameObject manager = GameObject.Find("RockslideManager");
            if (manager != null)
            {
                rockslideSpawner = manager.GetComponent<SpawnRockslide>();
            }

            if (rockslideSpawner == null)
            {
                Debug.LogWarning("RockslideManager or SpawnRockslide not found in scene.");
            }
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
        Debug.Log("Enemy distance: " + distanceToPlayer);

        if (distanceToPlayer <= attackRange)
            attackCoroutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        Debug.Log("AttackSequence START");
        isAttackSequenceRunning = true;
        isOnCooldown = true;

        if (playerShadowTarget != null)
            lockedTargetPosition = playerShadowTarget.position;
        else if (player != null)
            lockedTargetPosition = player.position;
        else if (playerCollider != null)
            lockedTargetPosition = playerCollider.bounds.center;

        if (enemyClass == EnemyClass.EarthGiant)
        {
            if (enemyType == EnemyType.Tree)
            {
                yield return RootAttack();
            }

            if (enemyType == EnemyType.Stone)
            {
                yield return StoneAttack();
            }

            if (enemyType == EnemyType.Ember)
            {
                yield return EmberAttack();
            }
        }
        else if (enemyClass == EnemyClass.Earthling)
        {
            yield return EarthlingAttack();
        }
        else if (enemyClass == EnemyClass.GiantElite)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (enemyType == EnemyType.Tree)
            {
                if (distance <= eliteMeleeRange)
                {
                    if (Random.value < 0.5f)
                        yield return EliteMelee();
                    else
                        yield return RootAttack();
                }
                else
                {
                    yield return RootAttack();
                }
            }
            else if (enemyType == EnemyType.Stone)
            {
                if (distance <= eliteMeleeRange)
                {
                    if (Random.value < 0.5f)
                        yield return EliteMelee();
                    else
                        yield return StoneAttack();
                }
                else
                {
                    yield return StoneAttack();
                }
            }
            else if (enemyType == EnemyType.Ember)
            {
                if (distance <= eliteMeleeRange)
                {
                    if (Random.value < 0.5f)
                        yield return EliteMelee();
                    else
                        yield return EmberAttack();
                }
                else
                {
                    yield return EmberAttack();
                }
            }
        }
        else if (enemyClass == EnemyClass.Boss)
        {
            yield return BossSequence();
        }

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

    private IEnumerator RootAttack()
    {
        if (movement != null)
            movement.SetAttacking(true);

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

        if (movement != null)
            movement.SetAttacking(false);

        yield return new WaitForSeconds(attackCooldown);
    }

    private IEnumerator StoneAttack()
    {
        if (movement != null)
            movement.SetAttacking(true);

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

        yield return new WaitForSeconds(stoneThrowDelay);

        if (attackDirection.sqrMagnitude > 0.001f && stoneProjectilePrefab != null && stoneThrowPoint != null)
        {
            Quaternion stoneRotation = Quaternion.LookRotation(attackDirection.normalized);
            GameObject spawnedStone = Instantiate(stoneProjectilePrefab, stoneThrowPoint.position, stoneRotation);

            HitBox hitBox = spawnedStone.GetComponentInChildren<HitBox>();
            if (hitBox != null)
                hitBox.SetDamageSource(this);

            canDamage = true;

            StoneProjectile proj = spawnedStone.GetComponent<StoneProjectile>();
            if (proj != null)
                proj.Initialize(
                    stoneThrowPoint.position,
                    lockedTargetPosition,
                    stoneArcHeight,
                    stoneTravelDuration,
                    damage,
                    damageKnockback,
                    this
                );
        }

        TrySpawnRandomRockslide();

        if (movement != null)
            movement.SetAttacking(false);

        yield return new WaitForSeconds(attackCooldown);

        canDamage = false;
    }

    private void TrySpawnRandomRockslide()
    {
        if (rockslideActive)
            return;

        if (rockslideSpawner == null)
            return;

        if (Random.value > rockslideProbability)
            return;

        rockslideActive = true;
        rockslideSpawner.Spawn();
        StartCoroutine(ReleaseRockslideLock());
    }

    private IEnumerator ReleaseRockslideLock()
    {
        yield return new WaitForSeconds(rockslideLock);
        rockslideActive = false;
    }

    private IEnumerator EmberAttack()
    {
        Debug.Log("Ember Attack sequence started.");
        yield break;
    }

    private IEnumerator EarthlingAttack()
    {
        Debug.Log("Earthling Attack sequence started.");
        yield break;
    }

    private IEnumerator EliteMelee()
    {
        yield break;
    }

    private IEnumerator BossSequence()
    {
        yield break;
    }

    public void OnHit(HealthManager targetHealth)
    {
        Debug.Log("Enemy OnHit called on: " + targetHealth.name);

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
            Transform shadow = player.Find("Player Shadow");
            if (shadow != null)
                playerShadowTarget = shadow;
        }

        if (movement != null)
            movement.player = player;
    }
}