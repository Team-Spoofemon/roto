using System.Collections;
using UnityEngine;

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
