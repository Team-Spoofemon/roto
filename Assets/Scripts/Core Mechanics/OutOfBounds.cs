using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private float damage = 99999f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OutOfBounds triggered by: {other.name}");
        var health = other.GetComponent<HealthManager>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
