public class MeleeWeaponController : MonoBehaviour
{
    [SerializeField] private Collider hitbox;
    [SerializeField] private Animator animator;
    [SerializeField] private string swingTrigger = "Melee";

    public void SetAnimator(Animator a) => animator = a; 

    private bool busy;

    public void Attack()
    {
        if (!busy) StartCoroutine(Melee());
    }

    private IEnumerator Melee()
    {
        busy = true;

        if (animator && !string.IsNullOrEmpty(swingTrigger))
            animator.SetTrigger(swingTrigger);

        yield return new WaitForSeconds(windup);

        if (hitbox) hitbox.enabled = true;
        yield return new WaitForSeconds(activeTime);
        if (hitbox) hitbox.enabled = false;

        yield return new WaitForSeconds(recovery);
        busy = false;
    }
}
