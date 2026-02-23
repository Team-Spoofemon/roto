using UnityEngine;
using TMPro;

public abstract class Interactable : MonoBehaviour
{
    [Header("Optional World Hover Text")]
    [SerializeField] private TextMeshPro worldHoverText;
    [SerializeField] private bool billboardToCamera = true;

    protected virtual void Awake()
    {
        if (worldHoverText == null)
            worldHoverText = GetComponentInChildren<TextMeshPro>(true);

        if (worldHoverText != null)
            worldHoverText.gameObject.SetActive(false);
    }

    protected virtual void LateUpdate()
    {
        if (!billboardToCamera || worldHoverText == null || !worldHoverText.gameObject.activeInHierarchy)
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        worldHoverText.transform.rotation =
            Quaternion.LookRotation(worldHoverText.transform.position - cam.transform.position);
    }

    public virtual string GetPromptText()
    {
        return "Press E to interact";
    }

    public void ShowHover()
    {
        if (worldHoverText == null) return;

        worldHoverText.text = GetPromptText();
        worldHoverText.gameObject.SetActive(true);
    }

    public void HideHover()
    {
        if (worldHoverText == null) return;

        worldHoverText.gameObject.SetActive(false);
    }

    public abstract void Interact();
}