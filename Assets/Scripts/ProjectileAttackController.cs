public class ProjectileAttackController : MonoBehaviour
{
    public float rotationSpeed = 1;
    public float ShotStrength = 50;
    public GameObject damageEffect;
    public GameObject Projectile;
    public Transform ShotPoint;
    private KeyCode fireKey;

    private bool fireRequested;

    public void Attack()
    {
        fireRequested = true;
    }

    private void Update()
    {
        float HorizontalRotation = Input.getAxis("Horizontal");
        float VerticalRotation = Input.getAxis("Vertical");

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles
            + new Vector3(0, HorizontalRotation * rotationSpeed, VerticalRotation * rotationSpeed)
            * Time.deltaTime);

        if (Input.GetKeyDown(fireKey))
        {
            this.Fire();
        }
        else if (fireRequested)
        {
            fireRequested = false;
            this.Fire();
        }
    }

    private Fire()
    {
        GameObject projectile = Instantiate(Projectile, ShotPoint.position, ShotPoint.rotation);
        projectile.GetComponent<Rigidbody>().velocity = ShotPoint.transform.up * ShotStrength;

        if (damageEffect)
        {
            Destroy(Instantiate(damageEffect, ShotPoint.position, ShotPoint.rotation), 2);
        }
    }
}