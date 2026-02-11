using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private float damage = 99999f;

    private void OnTriggerEnter(Collider other)
    {
        var root = other.transform.root;

        if (!root.CompareTag("Player"))
            return;

        var health = root.GetComponent<HealthManager>();
        if (health != null)
            health.TakeDamage(damage);
        else
            LevelManager.TriggerPlayerDeath();
    }
}
