using UnityEngine;
using UnityEngine.UI;

public class DamageBar : MonoBehaviour
{
    public Canvas barContainer;
    public Slider healthBar;
    public Slider flashBar;

    private void Update()
    {
        Quaternion rotation = Camera.main.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
    }
}
