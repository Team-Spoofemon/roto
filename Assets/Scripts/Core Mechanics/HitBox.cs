using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    private IHitHandler damageSource;

    private void Awake()
    {
        damageSource = GetComponentInParent<IHitHandler>();
    }

    public void SetDamageSource(IHitHandler source)
    {
        damageSource = source;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damageSource == null)
            return;

        if (((1 << other.gameObject.layer) & targetLayer.value) == 0)
            return;

        HealthManager targetHealth = other.GetComponentInParent<HealthManager>();

        if (targetHealth != null)
            damageSource.OnHit(targetHealth);
    }
}