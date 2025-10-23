using System;
using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private MonoBehaviour damageSourceBehaviour;
    private IHitHandler damageSource;

    private void Awake()
    {
        damageSource = damageSourceBehaviour as IHitHandler;
        if (damageSource == null && damageSourceBehaviour != null)
            Debug.LogError($"{name}: Assigned component does not implement IHitHandler.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer.value) != 0)
        {
            HealthManager targetHealth = other.GetComponent<HealthManager>();
            if (targetHealth)
            {
                Debug.Log("Hit: " + other.gameObject.name + "!");
                damageSource.OnHit(targetHealth);
            }
            else
                Debug.LogError(
                    gameObject.name
                        + " could not acquire "
                        + other.gameObject.name
                        + " HealthManager"
                );
        }
    }
}
