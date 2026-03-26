using System;
using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    private IHitHandler damageSource;

    private void Awake()
    {
        damageSource = GetComponentInParent<IHitHandler>();

        if (damageSource == null)
            Debug.LogError($"{name}: Could not find an IHitHandler in parents.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer.value) != 0)
        {
            HealthManager targetHealth = other.GetComponentInParent<HealthManager>();
            if ((targetHealth != null) && (damageSource != null))
            {
                damageSource.OnHit(targetHealth);
            }
        }
    }
}