using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    public TextMeshProUGUI promptLabel;
    private Interactable currentInteractable;
    private bool interactionCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            if (!interactionCooldown)
            {
                promptLabel.text = interactable.GetPromptText();
                promptLabel.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>() == currentInteractable)
        {
            currentInteractable = null;
            promptLabel.gameObject.SetActive(false);
        }
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
        promptLabel.gameObject.SetActive(false);
        yield return StartCoroutine(dialogueRoutine);
        yield return new WaitForSeconds(1f);
        if (currentInteractable != null)
        {
            promptLabel.text = currentInteractable.GetPromptText();
            promptLabel.gameObject.SetActive(true);
        }
        interactionCooldown = false;
    }
}
