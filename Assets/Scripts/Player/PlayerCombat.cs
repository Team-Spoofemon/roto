using System.Collections;
using System.Collections.Generic;
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

    //Following comments can be executed after implementation of the SpecialAbilityController
    //and AudioManager classes
    //[SerializeField] SpecialAbilityController specialAbilityController;
    //[SerializeField] AudioManager audioManager;

    public void OnMelee()
    {
        int swordIndex = Random.Range(0, swordSwingSounds.Length);
        // AudioClip swordClip = swordSwingSounds[swordIndex];
        // sword.PlayOneShot(swordClip);
        swordHitbox.enabled = true;
        playerAnim.SetTrigger("Melee");
        swordHitbox.enabled = false;
    }

    public void OnHit(HealthManager targetHealth)
    {
        CombatManager.Instance.SingleAttack(targetHealth, damage);
    }

    public void OnSpecialAbilityController()
    {
        //specialAbilityController.AreaOfEffect();
    }
}
