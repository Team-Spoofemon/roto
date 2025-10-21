using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TakeDamageHandler : MonoBehaviour
{
    private float __currentHealth;
    private bool __flashHealth = false;
    private bool __restoreColor = false;
    private bool __damageable = true;

    public bool hasDamageOverride = false;
    public bool hasDestroyedOverride = false;
    public bool showProgressBar = true;

    private DamageStats __damageableStats;

    [SerializeField]
    private UnityEvent<float> sig_takeDamageOverride = new UnityEvent<float>();

    [SerializeField]
    private UnityEvent sig_destroyedOverride = new UnityEvent();

    [SerializeField]
    private UnityEvent sig_destroyed = new UnityEvent();

    public GameObject targetObject;
    public Canvas progressBarContainer;
    public Slider whiteBar;
    public Slider healthBar;

    private void Awake()
    {
        if (progressBarContainer)
            progressBarContainer.gameObject.SetActive(showProgressBar);
    }

    private void Start()
    {
        __damageableStats = gameObject.GetComponent<DamageStats>();
        __currentHealth = __damageableStats.startingHealth;

        if (progressBarContainer)
            progressBarContainer.gameObject.SetActive(showProgressBar);
    }

    private void Update()
    {
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer)
        {
            if (__flashHealth)
            {
                targetRenderer.material.color = UnityEngine.Color.red;
                __restoreColor = true;
                __flashHealth = false;
            }
            else if (__restoreColor)
            {
                targetRenderer.material.color = UnityEngine.Color.white;
                __restoreColor = false;
            }
        }
    }

    public void TakeDamage(float damagePower)
    {
        if (hasDamageOverride)
        {
            sig_takeDamageOverride.Invoke(damagePower);
            return;
        }

        TakeDamageInternal(damagePower);
    }

    public void TakeDamageInternal(float damagePower)
    {
        if (!__damageable)
            return;

        float actualDamage = damagePower - __damageableStats.defense;
        if (actualDamage <= 0)
            return;

        progressBarContainer.gameObject.SetActive(showProgressBar);

        __currentHealth -= actualDamage;
        __flashHealth = true;
        if (healthBar != null && __damageableStats.startingHealth > 0f)
            healthBar.value = (__currentHealth / __damageableStats.startingHealth) * 100f;

        if (whiteBar != null && __damageableStats.startingHealth > 0f)
        {
            float targetPercent = (__currentHealth / __damageableStats.startingHealth) * 100f;
            whiteBar.value = targetPercent;
        }

        if (__currentHealth <= 0f)
            Destroyed();
    }

    public void Destroyed()
    {
        if (hasDamageOverride)
        {
            sig_destroyedOverride.Invoke();
            return;
        }
        DestroyedInternal();
    }

    public void DestroyedInternal()
    {
        __damageable = false;

        progressBarContainer.gameObject.SetActive(false);

        // SetDeathTexture();
        // StartCoroutine(DeathTexturePercentage(1f, 0f, 0.5f));
    }

    private void SignalDestroyed() => sig_destroyed.Invoke();

    private void SetDeathTexture()
    {
        if (!targetObject)
            return;
        var r = targetObject.GetComponent<Renderer>();
        if (!r)
            return;
    }

    private IEnumerator DeathTexturePercentage(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(from, to, t / duration);
            SetDeathPercentage(v);
            yield return null;
        }
        SetDeathPercentage(to);
        SignalDestroyed();
    }

    private void SetDeathPercentage(float value)
    {
        if (!targetObject)
            return;
        var r = targetObject.GetComponent<Renderer>();
        if (!r)
            return;

        if (r.material.HasProperty("_DyingTexturePercent"))
            r.material.SetFloat("_DyingTexturePercent", value);
    }
}
