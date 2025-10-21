using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EarthGiant : MonoBehaviour
{
    public void OnSigDestroyed()
    {
        Destroy(gameObject);
    }
}
