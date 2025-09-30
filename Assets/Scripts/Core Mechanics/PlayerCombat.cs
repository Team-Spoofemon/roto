using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public InputActionMap PlayerControls;

    public InputAction melee;
    //public InputAction SpecialAttack;
    
    void Awake(){
        //melee.performed += OnMelee;
        //SpecialAttack.performed += OnSpecialAttack;
    }

    void OnEnable(){
        melee.Enable();
        //SpecialAttack.Enable()

    }

    void OnDisable(){
        melee.Disable();
        //SpecialAttack.Disable()
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    /*function to melee
    player will press J to melee
    */

    public void OnMelee() 
    {
    }

    /*function to dodge?
    player will press the following:
    A + left arrow to dodge left
    S + down arrow to dodge down
    D + right arrow to dodge right
    */

    public void OnDodge() 
    {

    }

    /*function to reference special abilities class
    player will use j to activate special abilities controller
    SpecialAbilities.cs will handle this
    */

    public void OnSpecialAbilityController()
    {
        //reference special abilities class
    }


}
