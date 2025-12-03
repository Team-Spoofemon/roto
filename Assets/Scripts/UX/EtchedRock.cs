using UnityEngine;

public class EtchedRock : Interactable
{
    [TextArea] public string messageText;
    public float displayDuration = 4f;

    public override string GetPromptText()
    {
        return "Press E to read the etching";
    }

    public override void Interact()
    {
        PlayerInteractor interactor = FindAnyObjectByType<PlayerInteractor>();

        if (interactor != null)
        {
            interactor.RunDialogueCoroutine(
                DialogueManager.Instance.InstructionalText(messageText, displayDuration)
            );
        }
        else
        {
            Debug.LogError("No PlayerInteractor found in scene!");
        }
    }
}
