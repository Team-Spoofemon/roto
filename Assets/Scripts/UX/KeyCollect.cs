using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollect : MonoBehaviour
{

    [SerializeField] private GameObject key;
    public bool hasKey = false;

   public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            key.SetActive(false);
            hasKey = true;
            
            if (hasKey == true)
            {
                Debug.Log("Player has key.");
            }
        }
    }
}
