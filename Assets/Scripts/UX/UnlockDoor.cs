using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockDoor : MonoBehaviour
{
    [SerializeField] private KeyCollect keyCollect;
    [SerializeField] private Animator doorAnim;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")){
            if (keyCollect.hasKey == true)
            {
                OpenDoor();
                keyCollect.hasKey = false;
            }
        }
    }

    public void OpenDoor()
    {
            doorAnim.SetTrigger("Open");
    }

}
