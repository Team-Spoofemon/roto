using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
[Header("Health Settings")]
[SerializeField] private int maxHealth = 100;
[SerializeField] private int currentHealth;
[SerializeField] private float regenRate = 5f; // HP per second
[SerializeField] private float regenCooldown = 3f; // Time after hit before regen starts
[SerializeField] private float timeSinceLastHit = 0f;

[SerializeField] private bool isDead = false;

[Header("Attack Cooldowns")]
[SerializeField] private float specialAttackCooldown = 2f;
[SerializeField] private float specialAttackTimer = 0f;

// Start is called before the first frame update
void Start()
{
    currentHealth = maxHealth;
}

// Update is called once per frame
void Update()
{
    HandleRegen();
    HandleSpecialAttackCooldown();
}

void HandleRegen()
{
    if (currentHealth < maxHealth && !isDead)
    {
        timeSinceLastHit += Time.deltaTime;

        if (timeSinceLastHit >= regenCooldown)
        {
            currentHealth += Mathf.FloorToInt(regenRate * Time.deltaTime);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }
}

void HandleSpecialAttackCooldown()
{
    if (specialAttackTimer > 0f)
        specialAttackTimer -= Time.deltaTime;
}

public bool CanUseSpecial()
{
    return specialAttackTimer <= 0f;
}

public void UseSpecialAttack()
{
    if (CanUseSpecial())
        specialAttackTimer = specialAttackCooldown;
}

public void TakeDamage(int damage)
{
    if (isDead) return;

    currentHealth -= damage;
    timeSinceLastHit = 0f;

    if (currentHealth <= 0)
        Die();
}

public void HealFromHeart()
{
    if (isDead) return;

    int healAmount = Mathf.FloorToInt(maxHealth * 0.25f);
    currentHealth += healAmount;
    currentHealth = Mathf.Min(currentHealth, maxHealth);
}

void Die()
{
    isDead = true;
    Debug.Log("Player has died.");
}

public bool IsDead()
{
    return isDead;
}
}