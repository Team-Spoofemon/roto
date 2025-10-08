using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public PlayerCombat player;
    public Entity enemy;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.DealDamage(enemy.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            enemy.DealDamage(player.gameObject);
        }
    }
}
