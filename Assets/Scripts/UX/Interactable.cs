using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual string GetPromptText(){
        return "Press E to interact";
    }
    public abstract void Interact();
}
