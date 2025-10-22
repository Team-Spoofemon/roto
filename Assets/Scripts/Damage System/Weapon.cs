using UnityEngine;

public class Weapon : MonoBehaviour
{
    private EntityStats __baseStats;
    private float __damage;
    public LayerMask damageLayer;

    private void Start()
    {
        __baseStats = gameObject.GetComponent<EntityStats>();
    }

    private void onTriggerEnter(Collider other)
    {
        if (!((damageLayer.value & (1 << other.gameObject.layer)) == 0))
        {
            Debug.Log("Player collided with me!: " + other.gameObject.name);
            DamageManager enemy = other.GetComponent<DamageManager>();
            enemy.TakeDamage(__damage);
        }
    }
}
