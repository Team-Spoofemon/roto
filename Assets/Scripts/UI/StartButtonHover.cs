using UnityEngine;
using UnityEngine.EventSystems;

public class StartButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;

    [SerializeField] private string hoverTrigger = "Hover";
    [SerializeField] private string idleTrigger = "IdleStart";

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.ResetTrigger(idleTrigger);
        animator.SetTrigger(hoverTrigger);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.ResetTrigger(hoverTrigger);
        animator.SetTrigger(idleTrigger);
    }
}