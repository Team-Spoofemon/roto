using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IHitHandler
{
    [SerializeField] private Animator playerAnim;
    [SerializeField] private Collider swordHitbox;
    [SerializeField] private float damage;
    [SerializeField] private float damageKnockback;
    [SerializeField] private bool attackLockStatus;
    [SerializeField] private int amountOfAttacks;
    [SerializeField] AudioManager audioManager;

    //Following comments can be executed after implementation of the SpecialAbilityController
    //[SerializeField] SpecialAbilityController specialAbilityController;
    

    public void OnMelee()
    {
        if (attackLockStatus)
            return;

        AudioManager.Instance.PlaySwordSounds();

        StartCoroutine(MeleeRoutine());
    }

    private IEnumerator MeleeRoutine()
    {
        attackLockStatus = true;

        int attackNumber = Random.Range(0,amountOfAttacks);
        //Random.Range will pick a number between 0 and one less than amountOfAttacks
        if(attackNumber == 0)
        {
            yield return StartCoroutine(CombatManager.Instance.PlayAttackAndLock(swordHitbox, playerAnim, "Melee"));
        }
        if(attackNumber == 1)
        {
            yield return StartCoroutine(CombatManager.Instance.PlayAttackAndLock(swordHitbox, playerAnim, "MeleeUp"));
        }
        if(attackNumber == 2)
        {
            yield return StartCoroutine(CombatManager.Instance.PlayAttackAndLock(swordHitbox, playerAnim, "SpecialStab"));
        }
        
        attackLockStatus = false;
    }

    public void OnHit(HealthManager targetHealth)
    {
        CombatManager.Instance.SingleAttack(targetHealth, damage, transform, damageKnockback);
    }

    public void OnSpecialAbilityController()
    {
        //specialAbilityController.AreaOfEffect();
    }
}
