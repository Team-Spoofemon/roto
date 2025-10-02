public class MeleeAttack : MonoBehavior
{
    // This trigger event will preform the attack action
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable hit = other.GetComponent<IDamageable>();
        if (hit != null)
        {
            hit.Damage();
        }
    }
}