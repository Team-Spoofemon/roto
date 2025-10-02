public class AttackDamage : MonoBehaviour
{
    // This trigger event will preform the attack action. Place on a weapon to apply damage.
    private void OnTriggerEnter(Collider other)
    {
        IDamageable hit = other.GetComponent<IDamageable>();
        if (hit != null)
        {
            hit.Damage();
        }
    }
}