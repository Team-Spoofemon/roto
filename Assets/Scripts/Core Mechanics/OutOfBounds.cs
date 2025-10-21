using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public int damage = 99999;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
