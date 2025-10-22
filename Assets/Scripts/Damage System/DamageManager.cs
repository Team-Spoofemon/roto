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
    private EntityStats _baseStats;

    // Dynamically pulled stat variables
    private float _totalHealth;
    private float _totalDefense;
    public float _currentHealth;
    private float _currentDefense;

    // Damage logic variables
    // private bool _takingDamage = false;
    private bool _damageable = true;

    // Health Bar Components
    private DamageBar _damageBar;

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
        _baseStats = gameObject.GetComponent<EntityStats>();
        _totalHealth = _baseStats.baseHealth;
        _totalDefense = _baseStats.baseDefense;
        _currentHealth = _totalHealth;

        _damageBar = GetComponentInChildren<DamageBar>();

        // Progress bar to be toggle
        if (_damageBar)
        {
            _damageBar.SetActiveStatus(showHealthBar);
            _damageBar.UpdateHealthSlider(_currentHealth, _totalHealth);
        }
    }

    private void Update()
    {
        if (_damageBar)
        {
            _damageBar.UpdateHealthSlider(_currentHealth, _totalHealth);
        }
        if (_currentHealth <= 0 && _damageable)
            HandleDestory();
    }

    public void TakeDamage(float takenDamage)
    {
        Debug.Log("HAS BEEN DAMAGED");
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
        if (!_damageable)
            return;
        float actualDamage = takenDamage - _totalDefense;
        if (actualDamage <= 0)
            return;

        // Actual damaging
        _currentHealth -= actualDamage;
        // _takingDamage = true;

        if (_damageBar && _baseStats.baseHealth > 0f)
            _damageBar.UpdateHealthSlider(_currentHealth, _totalHealth);

        if (_currentHealth <= 0)
            HandleDestory();
    }

    public void HandleDestory()
    {
        _damageable = false;
        if (hasDeathOverride)
        {
            sig_DestroyedOverride.Invoke();
            return;
        }
        HandleDestoryInternal();
    }

    private async void HandleDestoryInternal()
    {
        if (_damageBar)
            _damageBar.SetActiveStatus(false);

        if (hasDeathAnimation)
        {
            if (hasDeathAnimationOverride)
                sig_DeathAnimationOverride.Invoke();
            else
                await DeathAnimationAsync(0.5f, 0.9f);
        }

        Destroy(this.gameObject);
    }

    // Zelda-like death animation where they shrink, turn red, and spin down.
    private async Task DeathAnimationAsync(float sec, float spinSpeed)
    {
        if (this == null)
            return;
        Transform tr = transform;
        if (tr == null)
            return;

        // Cache renderers once
        var renderers = GetComponentsInChildren<Renderer>();

        // Compute initial combined bounds and remember the original ground Y (min.y)
        Bounds initialBounds = default;
        bool hasInitial = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null)
                continue;
            if (!hasInitial)
            {
                initialBounds = r.bounds;
                hasInitial = true;
            }
            else
                initialBounds.Encapsulate(r.bounds);
        }
        float groundY = hasInitial ? initialBounds.min.y : tr.position.y;

        Vector3 startScale = tr.localScale;
        Quaternion startRot = tr.rotation;

        // Turn the whole object (and children) red
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
            if (this == null || tr == null)
                return;

            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / duration);

            // Shrink
            tr.localScale = Vector3.Lerp(startScale, Vector3.zero, t01);

            // Spin
            spinAccum += Mathf.Max(0f, spinSpeed) * 360f * Time.deltaTime;
            tr.rotation = startRot * Quaternion.Euler(0f, spinAccum, 0f);

            // Keep the bottom glued to original ground Y by adjusting Y position
            if (hasInitial)
            {
                Bounds now = default;
                bool hasNow = false;
                for (int i = 0; i < renderers.Length; i++)
                {
                    var r = renderers[i];
                    if (r == null)
                        continue;
                    if (!hasNow)
                    {
                        now = r.bounds;
                        hasNow = true;
                    }
                    else
                        now.Encapsulate(r.bounds);
                }
                if (hasNow)
                {
                    float dy = groundY - now.min.y; // raise/lower so min.y stays constant
                    tr.position += new Vector3(0f, dy, 0f);
                }
            }

            await Task.Yield();
        }

        if (this == null || tr == null)
            return;

        // Ensure final state and final ground clamp
        tr.localScale = Vector3.zero;
        tr.rotation = startRot * Quaternion.Euler(0f, spinAccum, 0f);

        if (hasInitial)
        {
            Bounds now = default;
            bool hasNow = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null)
                    continue;
                if (!hasNow)
                {
                    now = r.bounds;
                    hasNow = true;
                }
                else
                    now.Encapsulate(r.bounds);
            }
            if (hasNow)
            {
                float dy = groundY - now.min.y;
                tr.position += new Vector3(0f, dy, 0f);
            }
        }

        sig_Death?.Invoke();
    }
}
