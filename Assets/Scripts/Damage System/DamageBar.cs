using UnityEngine;
using UnityEngine.UI;

public class DamageBar : MonoBehaviour
{
    [SerializeField]
    private Canvas _barContainer;

    [SerializeField]
    public Slider _healthBar;

    [SerializeField]
    public Slider _flashBar;

    private float _currentHealth;
    private float _totalHealth;
    private bool _activeStatus = false;

    public void UpdateHealthSlider(float currentHealth, float totalHealth)
    {
        if (_healthBar)
        {
            // update health status on bar side
            _currentHealth = currentHealth;
            _totalHealth = totalHealth;

            // Verify modfier is w/in (0,100]% and bar is within [0,100]
            if (_healthBar.value >= 0 && _healthBar.value <= 100)
                _healthBar.value = currentHealth / totalHealth;
        }
    }

    public void SetActiveStatus(bool status)
    {
        if (_barContainer)
            _barContainer.gameObject.SetActive(status);
    }

    private void Update()
    {
        if (_activeStatus)
        {
            Quaternion rotation = Camera.main.transform.rotation;
            transform.LookAt(
                transform.position + rotation * Vector3.forward,
                rotation * Vector3.up
            );
        }
    }
}
