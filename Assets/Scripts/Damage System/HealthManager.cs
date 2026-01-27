using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Entity Instance Stat Modification")]
    [SerializeField] private float totalHealth;
    [SerializeField] private float totalDefense;
    [SerializeField] private float damageInvulnerabilityTime = 0.7f;

    [SerializeField] private float currentHealth;
    public float currentDefense { get; private set; }

    private bool _damageable = true;

    private DamageBar _damageBar;

    [Header("Damage System Overrides")]
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private bool hasDeathOverride = false;
    [SerializeField] private bool hasDamageOverride = false;

    [Header("System Override Signals (only enable if bools are checked)")]
    public FloatEvent sig_TakeDamageOverride = new FloatEvent();
    public UnityEvent sig_DestroyedOverride = new UnityEvent();

    [Header("Default signals, must be attached with entity class death function")]
    public UnityEvent sig_Death = new UnityEvent();

    [Header("Animation Settings")]
    [SerializeField] private bool hasDeathAnimation = false;
    [SerializeField] private bool hasDeathAnimationOverride = false;
    [SerializeField] private bool flashOnInvulnerable = true;

    public UnityEvent sig_DeathAnimationOverride = new UnityEvent();

    private Coroutine _invulnRoutine;
    private SpriteRenderer[] sprites;

    private bool _deathHandled;

    private void Start()
    {
        currentHealth = totalHealth;

        _damageBar = GetComponentInChildren<DamageBar>();

        if (_damageBar)
        {
            _damageBar.SetActiveStatus(showHealthBar);
            _damageBar.UpdateHealthSlider(currentHealth, totalHealth);
        }

        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (_damageBar)
            _damageBar.UpdateHealthSlider(currentHealth, totalHealth);

        if (!_deathHandled && currentHealth <= 0f)
            HandleDestory();
    }

    public void TakeDamage(float damage)
    {
        if (hasDamageOverride)
        {
            sig_TakeDamageOverride.Invoke(damage);
            return;
        }

        TakeDamageInternal(damage);
    }

    private void TakeDamageInternal(float damage)
    {
        if (!_damageable || _deathHandled)
            return;

        float actualDamage = damage - totalDefense;
        if (actualDamage <= 0f)
            return;

        currentHealth -= actualDamage;

        if (currentHealth > 0f && damageInvulnerabilityTime > 0f)
        {
            if (_invulnRoutine != null)
            {
                StopCoroutine(_invulnRoutine);
                _invulnRoutine = null;
            }

            _invulnRoutine = StartCoroutine(
                InvulnerabilityCoroutine(
                    sprites,
                    damageInvulnerabilityTime,
                    0.05f,
                    flashOnInvulnerable
                )
            );
        }

        if (_damageBar && totalHealth > 0f)
            _damageBar.UpdateHealthSlider(currentHealth, totalHealth);

        if (currentHealth <= 0f)
            HandleDestory();
    }

    private IEnumerator InvulnerabilityCoroutine(
        SpriteRenderer[] sprites,
        float duration,
        float flashPeriod,
        bool useAlpha = false
    )
    {
        _damageable = false;

        float endTime = Time.time + Mathf.Max(0f, duration);
        bool flashOn = false;

        if (useAlpha && sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i])
                    sprites[i].color = new Color(
                        sprites[i].color.r,
                        sprites[i].color.g,
                        sprites[i].color.b,
                        1f
                    );
            }
        }
        else if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i])
                    sprites[i].enabled = true;
        }

        while (Time.time < endTime)
        {
            flashOn = !flashOn;

            if (sprites != null)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    var sr = sprites[i];
                    if (!sr)
                        continue;

                    if (useAlpha)
                    {
                        var c = sr.color;
                        c.a = flashOn ? 0.5f : 1f;
                        sr.color = c;
                    }
                    else
                    {
                        sr.enabled = flashOn;
                    }
                }
            }

            yield return new WaitForSeconds(flashPeriod);
        }

        if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                var sr = sprites[i];
                if (!sr)
                    continue;

                if (useAlpha)
                {
                    var c = sr.color;
                    c.a = 1f;
                    sr.color = c;
                }
                else
                {
                    sr.enabled = true;
                }
            }
        }

        _damageable = true;
        _invulnRoutine = null;
    }

    public void HandleDestory()
    {
        if (_deathHandled)
            return;

        _deathHandled = true;

        if (_invulnRoutine != null)
        {
            StopCoroutine(_invulnRoutine);
            _invulnRoutine = null;
        }

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

        if (CompareTag("Player"))
        {
            LevelManager.TriggerPlayerDeath();
            return;
        }

        if (hasDeathAnimation)
        {
            if (hasDeathAnimationOverride)
                sig_DeathAnimationOverride.Invoke();
            else
                await DeathAnimationAsync(0.5f, 0.9f);
        }

        sig_Death?.Invoke();

        Destroy(gameObject);
    }

    private async Task DeathAnimationAsync(float sec, float spinSpeed)
    {
        if (this == null)
            return;

        Transform tr = transform;
        if (tr == null)
            return;

        var renderers = GetComponentsInChildren<Renderer>();

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
            {
                initialBounds.Encapsulate(r.bounds);
            }
        }

        float groundY = hasInitial ? initialBounds.min.y : tr.position.y;

        Vector3 startScale = tr.localScale;
        Quaternion startRot = tr.rotation;

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

            tr.localScale = Vector3.Lerp(startScale, Vector3.zero, t01);

            spinAccum += Mathf.Max(0f, spinSpeed) * 360f * Time.deltaTime;
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
                    {
                        now.Encapsulate(r.bounds);
                    }
                }

                if (hasNow)
                {
                    float dy = groundY - now.min.y;
                    tr.position += new Vector3(0f, dy, 0f);
                }
            }

            await Task.Yield();
        }

        if (this == null || tr == null)
            return;

        tr.localScale = Vector3.zero;
        tr.rotation = startRot * Quaternion.Euler(0f, spinAccum, 0f);

        sig_Death?.Invoke();
    }
}
