using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing the damage handeling of a the GameObject it is placed on.
/// This should be used in tandum with some Entity Script (Enemy/DamagableObject/Player), anything that can be damaged.
/// </summary>
public class DamageManager : MonoBehaviour
{
    [Header("Entity Instance Stat Modification")]
    // Entity stats must be attached separately to the same Game Object
    private EntityStats __baseStats;

    // Dynamically pulled stat variables
    private float __totalHealth;
    private float __totalDefense;
    public float __currentHealth;
    private float __currentDefense;

    // Damage logic variables
    private bool __takingDamage = false;
    private bool __damageable = true;

    // [Header("Health Bar Components")]
    private DamageBar __damageBar;
    private Canvas __barContainer;
    private Slider __healthBar;
    private Slider __flashBar;

    [Header("Damage System Overrides")]
    public bool showHealthBar = true;
    public bool hasDeathOverride = false;
    public bool hasDamageOverride = false;

    [Header("System Override Signals (only enable if bools are checked)")]
    public UnityEvent<float> sig_TakeDamageOverride = new UnityEvent<float>();
    public UnityEvent sig_DestroyedOverride = new UnityEvent();

    [Header("Default signals, must be attached with entity class death function")]
    public UnityEvent sig_Death = new UnityEvent();

    [Header("Death Animation Settings")]
    public bool hasDeathAnimation = false;
    public bool hasDeathAnimationOverride = false;
    public UnityEvent sig_DeathAnimationOverride = new UnityEvent();

    private void Start()
    {
        // Set Stats during runtime
        __baseStats = gameObject.GetComponent<EntityStats>();
        __totalHealth = __baseStats.baseHealth;
        __totalDefense = __baseStats.baseDefense;
        __currentHealth = __totalHealth;

        __damageBar = GetComponentInChildren<DamageBar>();

        // Progress bar to be toggle
        if (__damageBar)
        {
            __healthBar = __damageBar.healthBar;
            __flashBar = __damageBar.flashBar;
            __barContainer = __damageBar.barContainer;
            __barContainer.gameObject.SetActive(showHealthBar);
            UpdateSlider();
        }
    }

    private void Update()
    {
        if (__damageBar)
            UpdateSlider();

        if (__currentHealth <= 0)
            HandleDestory();
    }

    public void UpdateSlider()
    {
        if (__healthBar)
        {
            // Verify modfier is w/in (0,100]% and bar is within [0,100]
            if (__healthBar.value >= 0 && __healthBar.value <= 100)
                __healthBar.value = __currentHealth / __totalHealth;
        }
    }

    public void TakeDamage(float takenDamage)
    {
        if (hasDamageOverride)
        {
            // Alert override function
            sig_TakeDamageOverride.Invoke(takenDamage);
            return;
        }
        TakeDamageInternal(takenDamage);
    }

    private void TakeDamageInternal(float takenDamage)
    {
        if (!__damageable)
            return;
        float actualDamage = takenDamage - __totalDefense;
        if (actualDamage <= 0)
            return;

        // Actual damaging
        __currentHealth -= actualDamage;
        __takingDamage = true;

        if (__healthBar && __baseStats.baseHealth > 0f)
            UpdateSlider();

        if (__currentHealth <= 0)
            HandleDestory();
    }

    public void HandleDestory()
    {
        if (hasDeathOverride)
        {
            sig_DestroyedOverride.Invoke();
            return;
        }
        HandleDestoryInternal();
    }

    private async void HandleDestoryInternal()
    {
        __damageable = false;
        if (__barContainer)
            __barContainer.gameObject.SetActive(false);

        if (hasDeathAnimation)
        {
            if (hasDeathAnimationOverride)
                sig_DeathAnimationOverride.Invoke();
            else
                await DeathAnimationAsync(0.8f, 0.9f);
        }

        Destroy(this.gameObject);
    }

    // Zelda-like death animation where they shrink, turn red, and spin down.
    private async Task DeathAnimationAsync(float sec, float spinSpeed)
    {
        Transform tr = transform;
        Vector3 startScale = tr.localScale;
        Quaternion startRot = tr.rotation;

        // Turn the whole object (and children) red
        var renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r != null && r.material != null && r.material.HasProperty("_Color"))
                r.material.color = Color.red;
        }

        float duration = Mathf.Max(0.01f, sec);
        float elapsed = 0f;
        float spinAccum = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / duration);

            // Shrink to zero over the duration
            tr.localScale = Vector3.Lerp(startScale, Vector3.zero, t01);

            // Spin continuously around local up axis
            spinAccum += Mathf.Max(0f, spinSpeed) * 360f * Time.deltaTime;
            tr.rotation = startRot * Quaternion.Euler(0f, spinAccum, 0f);

            await Task.Yield(); // wait until next frame (non-blocking)
        }

        // Ensure final state
        tr.localScale = Vector3.zero;
        tr.rotation = startRot * Quaternion.Euler(0f, spinAccum, 0f);

        // Notify listeners that death has completed
        sig_Death?.Invoke();
    }
}
