using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField]
    private int attackDamage = 10;

    [SerializeField]
    private float attackRange = 0.9f; // radius of the swing

    [SerializeField]
    private Transform attackOrigin; // a child in front of the player

    [SerializeField]
    private LayerMask damageableMask; // set to your "Damageable" (e.g., layer 5)

    private TakeDamageHandler myHandler;

    // small reusable buffer to avoid GC (immediate “shape cast” like Godot)
    private static readonly Collider[] hitBuffer = new Collider[16];

    void Awake()
    {
        // use component lookup, not names/paths
        myHandler = GetComponentInChildren<TakeDamageHandler>();
    }

    void Update()
    {
        // simplest trigger: left click or J — or call Attack() from your sword animation event
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
            UnityEngine.Debug.Log("PLAYER HITS");
        Attack();
    }

    public void Attack()
    {
        Vector3 center = attackOrigin ? attackOrigin.position : transform.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            attackRange,
            hitBuffer,
            damageableMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            var c = hitBuffer[i];
            var targetHandler = c.GetComponentInParent<TakeDamageHandler>();
            if (targetHandler != null && targetHandler != myHandler)
            {
                // no hard-coded node names or paths—just the component
                targetHandler.TakeDamage(attackDamage);
                break; // stop after first valid hit (keep it simple)
            }
        }
    }

    // Let other systems damage the player via the same handler
    public void ReceiveDamage(float amount)
    {
        if (myHandler != null)
            myHandler.TakeDamage(amount);
    }

    void OnDrawGizmosSelected()
    {
        if (!attackOrigin)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRange);
    }
}
