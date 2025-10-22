using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OLDPlayerCombat : IDamages
{
    [Header("Attack Cooldowns")]
    public float specialAttackCooldown = 2f;
    private float specialAttackTimer = 0f;

    [Header("Damage")]
    public int attackDamage = 10;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        HandleSpecialAttackCooldown();
    }

    /*function to melee
    player will press J to melee
    */

    public void OnMelee() { }

    /*function to dodge?
    player will press the following:
    A + left arrow to dodge left
    S + down arrow to dodge down
    D + right arrow to dodge right
    */

    public void OnDodge() { }

    /*function to reference special abilities class
    player will use j to activate special abilities controller
    SpecialAbilities.cs will handle this
    */

    public void OnSpecialAbilityController()
    {
        //reference special abilities class
    }

    void HandleSpecialAttackCooldown()
    {
        if (specialAttackTimer > 0f)
            specialAttackTimer -= Time.deltaTime;
    }

    public bool CanUseSpecial() => specialAttackTimer <= 0f;

    public void UseSpecialAttack()
    {
        if (CanUseSpecial())
            specialAttackTimer = specialAttackCooldown;
    }

    public void DealDamage(GameObject target)
    {
        var entityHealth = target.GetComponent<IDamageable>();
        if (entityHealth != null)
        {
            entityHealth.TakeDamage(attackDamage);
        }
        else
        {
            UnityEngine.Debug.Log($"{target.name} is not a damageable GameObject.");
        }
    }
}
