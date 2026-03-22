using UnityEngine;
using System.Collections;

public class EtchedRock : Interactable
{
    [TextArea] public string messageText;
    public float displayDuration = 4f;

    [SerializeField] private GameObject interactionIndicator;

    private bool hasInteracted;
    private bool isOnCooldown;

    protected override void Awake()
    {
        base.Awake();

        if (interactionIndicator != null)
            interactionIndicator.SetActive(!hasInteracted);
    }

    public override string GetPromptText()
    {
        return "Press E to read the etching";
    }

    public override void Interact()
    {
        if (isOnCooldown) return;

        if (!hasInteracted)
        {
            hasInteracted = true;

            if (interactionIndicator != null)
                interactionIndicator.SetActive(false);
        }

        PlayerInteractor interactor = FindAnyObjectByType<PlayerInteractor>();

        if (interactor != null)
        {
            interactor.RunDialogueCoroutine(
                DialogueManager.Instance.InstructionalText(messageText, displayDuration)
            );

            StartCoroutine(CooldownRoutine());
        }
        else
        {
            Debug.LogError("No PlayerInteractor found in scene!");
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(displayDuration + 5f);
        isOnCooldown = false;
    }
}