using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    //public EnemyHealth enemyHealth; <-- Remove comment once EnemyHealth.cs created
    //public SpecialAbilities specialAbilities; <-- Remove comment once SpecialAbilties.cs created

    public void OnMelee() 
    {
        
        //enemyHealth.TakeDamage();
        //insert sound effect
        //insert animation randomizer
        Debug.Log("Sword Sound Effect");
        Debug.Log("Sword Animation");
    }

    public void OnSpecialAbilityController()
    {
        //specialAbilities.areaOfEffect();
    }


}
