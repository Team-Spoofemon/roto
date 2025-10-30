using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IHitHandler
{
    [SerializeField]
    private Animator playerAnim;

    [SerializeField]
    private Collider swordHitbox;

    [SerializeField]
    private AudioSource sword;

    [SerializeField]
    private AudioClip[] swordSwingSounds;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageKnockback;
    private bool attackLockStatus;

    //Following comments can be executed after implementation of the SpecialAbilityController
    //and AudioManager classes
    //[SerializeField] SpecialAbilityController specialAbilityController;
    //[SerializeField] AudioManager audioManager;

    public void OnMelee()
    {
        if (attackLockStatus)
            return;

        //Access AudioManager.PlaySwordSounds()

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
