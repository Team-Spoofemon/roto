using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private int damage = 99999;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OutOfBounds triggered by: {other.name}");
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }

}
