using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [SerializeField]
    private Collider swordHitbox;
    private PlayerCombat playerCombat;

    //private EnemyHealth enemyHealth;

    private void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //other.GetComponent<EnemyHealth>().TakeDamage(playerCombat.AttackDamage);
        }
    }
}
