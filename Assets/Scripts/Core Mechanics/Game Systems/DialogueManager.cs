using System.Collections;
using UnityEngine;

/// <summary>
/// DialogueManager is the controller for dialogue flow.
/// It just tells UIManager what kind of dialogue to show and in what order.
///
/// How to use it:
/// - Put this on a GameObject in the starting scene so it becomes DialogueManager.Instance.
/// - UIManager must already exist, since this forwards everything to it.
///
/// Dialogue helpers:
/// - InstructionalText(text, duration): shows text with no speaker name and auto-hides.
/// - CharacterDialogue(name, lines): plays multiple lines in order, waits for clicks between each.
/// - Narration(text, duration): shows narration-style text and auto-hides.
///
/// General controls:
/// - HideText(): instantly hides the dialogue box.
/// - ResetDialogue(): stops all dialogue coroutines and fully clears the dialogue UI.
///
/// Notes:
/// - typeSpeed controls how fast text types out globally for dialogue.
/// - This class is basically a convenience layer so other systems don’t have to talk
///   directly to UIManager or worry about DialogueType.
/// </summary>


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private UIManager uiManager;
    public float typeSpeed = 0.03f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
    }

    public IEnumerator InstructionalText(string text, float duration)
    {
        yield return uiManager.ShowDialogueLine("", text, typeSpeed, DialogueType.Instruction, duration);
    }

    public IEnumerator CharacterDialogue(string name, string[] lines)
    {
        foreach (string line in lines)
            yield return uiManager.ShowDialogueLine(name, line, typeSpeed, DialogueType.Character, 0f);
    }

    public IEnumerator Narration(string text, float duration)
    {
        yield return uiManager.ShowDialogueLine("", text, typeSpeed, DialogueType.Narration, duration);
    }

    public void HideText()
    {
        uiManager.HideTextbox();
    }

    public void ResetDialogue()
    {
        StopAllCoroutines();
        uiManager.ResetDialogueUI();
    }
}
