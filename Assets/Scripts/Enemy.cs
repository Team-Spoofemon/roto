public abstract class Enemy : MonoBehaviour
{
    [Header("Common Attack Refs")]
    [SerializeField] protected ProjectileAttackController rangeWeapon;
    [SerializeField] protected MeleeWeaponController meleeWeapon;
    public abstract void Attack();

    protected virtual void RangeAttack()
    {
        rangeWeapon?.Attack();
    }
    protected virtual void MeleeAttack()
    {
        meleeWeapon?.Attack();
    }
}