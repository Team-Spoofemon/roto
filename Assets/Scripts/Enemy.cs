public abstract class Enemy : MonoBehaviour
{
    [Header("Common Attack Refs")]
    [SerializeField] protected ProjectileAttackController rangeWeapon;
    [SerializeField] protected MeleeWeaponController meleeWeapon;
    
    [Header("Animation")]
    [SerializeField] protected Animator animator;

    public abstract void Attack();

    protected virtual void RangeAttack()
    {
        rangeWeapon?.Attack(animator);
    }
    protected virtual void MeleeAttack()
    {
        meleeWeapon?.Attack(animator);
    }
}