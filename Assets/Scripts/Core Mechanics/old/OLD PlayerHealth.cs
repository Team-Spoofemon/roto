using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OLDPlayerHealth : MonoBehaviour, IDamageable, IKillable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public float regenRate = 5f; // HP per second
    public float regenCooldown = 3f; // Time after hit before regen starts
    private float timeSinceLastDamage = 0f;
    private bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        HandleRegen();
    }

    void HandleRegen()
    {
        if (currentHealth < maxHealth && !isDead)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenCooldown)
            {
                currentHealth += Mathf.FloorToInt(regenRate * Time.deltaTime);
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        timeSinceLastDamage = 0f;

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int healAmount)
    {
        if (isDead)
            return;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void Die()
    {
        isDead = true;
        Debug.Log($"Player has been killed");
        // SceneManager.LoadScene(0);   // Return to main menu
    }
}
