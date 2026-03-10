using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockKeySystem : MonoBehaviour
{
    public bool hasKey = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")){
        Destroy(gameObject);
        hasKey = true;
        }
    }
}
