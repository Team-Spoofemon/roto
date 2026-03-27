using UnityEngine;

public class DamageBar : MonoBehaviour
{
    [SerializeField]
    private Canvas _barContainer;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Sprite[] healthSprites; // 0 = empty, last = full

    private int maxIndex;

    private void Awake()
    {
        if (healthSprites != null && healthSprites.Length > 0)
            maxIndex = healthSprites.Length - 1;
    }

    public void UpdateHealthSlider(float currentHealth, float totalHealth)
    {
        if (_spriteRenderer == null || healthSprites.Length == 0)
            return;

        float percent = currentHealth / totalHealth;
        percent = Mathf.Clamp01(percent);

        int index = Mathf.RoundToInt(percent * maxIndex);
        index = Mathf.Clamp(index, 0, maxIndex);

        _spriteRenderer.sprite = healthSprites[index];
    }

    public void SetActiveStatus(bool status)
    {
        if (_barContainer)
            _barContainer.gameObject.SetActive(status);
    }

    private void Update()
    {
        if (_barContainer)
        {
            Quaternion rotation = Camera.main.transform.rotation;
            transform.LookAt(
                transform.position + rotation * Vector3.forward,
                rotation * Vector3.up
            );
        }
    }
}