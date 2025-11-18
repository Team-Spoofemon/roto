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
        yield return StartCoroutine(
            CombatManager.Instance.PlayAttackAndLock(swordHitbox, playerAnim, "Melee")
        );
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
