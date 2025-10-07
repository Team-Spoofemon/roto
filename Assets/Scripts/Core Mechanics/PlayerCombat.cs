using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    //[SerializeField] private Animator playerAnim;
    [SerializeField] private Collider swordHitbox;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioSource sword;
    [SerializeField] private AudioClip[] swordSwingSounds;
    [SerializeField] public float attackDamage = 5.0f;

    //Following comments can be executed after implementation of the SpecialAbilityController
    //and AudioManager classes
    //[SerializeField] SpecialAbilityController specialAbilityController;
    //[SerializeField] AudioManager audioManager;

    public void OnMelee() 
    {
        int swordIndex = Random.Range(0, swordSwingSounds.Length);
        AudioClip swordClip = swordSwingSounds[swordIndex];
        sword.PlayOneShot(swordClip);
        swordHitbox.enabled = true;
        Debug.Log("Sword Swinging");
        //playerAnim.SetTrigger("Melee");
        swordHitbox.enabled = false;
        Debug.Log("Sword Swung");

    }

    public void OnSpecialAbilityController()
    {
        //specialAbilityController.AreaOfEffect();
    }


}
