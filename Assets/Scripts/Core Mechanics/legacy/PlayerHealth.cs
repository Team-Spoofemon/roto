/*using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float regenRate = 5f;
    [SerializeField] private float regenCooldown = 3f;
    [SerializeField] private float timeSinceLastHit = 0f;
    [SerializeField] private bool isDead = false;

    [Header("Attack Cooldowns")]
    [SerializeField] private float specialAttackCooldown = 2f;
    [SerializeField] private float specialAttackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
    }

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

    public bool CanUseSpecial() => specialAttackTimer <= 0f;

    public void UseSpecialAttack()
    {
        if (CanUseSpecial())
            specialAttackTimer = specialAttackCooldown;
    }

    public void TakeDamage(int damage)
{
    Debug.Log($"TakeDamage called. CurrentHealth={currentHealth}, Damage={damage}, isDead={isDead}");
    
    if (isDead)
    {
        Debug.Log("TakeDamage: Player is already dead, ignoring damage.");
        return;
    }

    currentHealth -= damage;
    timeSinceLastHit = 0f;
    Debug.Log($"After damage: CurrentHealth={currentHealth}");

    if (currentHealth <= 0)
    {
        Debug.Log("Health <= 0 — calling Die()");
        Die();
    }
}


    public void HealFromHeart()
    {
        if (isDead)
            return;
        int healAmount = Mathf.FloorToInt(maxHealth * 0.25f);
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    void Die()
    {
        isDead = true;
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
            levelManager.OnPlayerDeath();
        Debug.Log("PlayerHealth.Die() called");
        Debug.Log($"LevelManager found? {levelManager != null}");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public bool IsDead() => isDead;
}
*/