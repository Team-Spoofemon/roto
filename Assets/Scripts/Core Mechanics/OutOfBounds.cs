using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damage = 99999;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("You have fallen to your death.");
        }
    }
}
