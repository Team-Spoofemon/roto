using UnityEngine;
using System.Collections;
using TMPro;
public class PlayerInteractor : MonoBehaviour
{
    private Interactable currentInteractable;
    private bool interactionCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable == null) return;

        currentInteractable = interactable;

        if (!interactionCooldown && UIManager.Instance != null)
        {
            var text = interactable.GetPromptText();
            if (!string.IsNullOrEmpty(text))
                UIManager.Instance.ShowPrompt(text);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentInteractable == null) return;

        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable != currentInteractable) return;

        currentInteractable = null;

        if (UIManager.Instance != null)
            UIManager.Instance.HidePrompt();
    }

    private void Update()
    {
        if (!interactionCooldown && currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    public void RunDialogueCoroutine(IEnumerator routine)
    {
        StartCoroutine(DialogueCooldownRoutine(routine));
    }

    private IEnumerator DialogueCooldownRoutine(IEnumerator dialogueRoutine)
    {
        interactionCooldown = true;

        if (UIManager.Instance != null)
            UIManager.Instance.HidePrompt();

        yield return StartCoroutine(dialogueRoutine);
        yield return new WaitForSeconds(1f);

        if (currentInteractable != null && UIManager.Instance != null)
        {
            var text = currentInteractable.GetPromptText();
            if (!string.IsNullOrEmpty(text))
                UIManager.Instance.ShowPrompt(text);
        }

        interactionCooldown = false;
    }
}