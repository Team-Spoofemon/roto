using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private UIManager uiManager;

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

    public void InstructionalText(string text)
    {
        uiManager.DisplayTextbox(text);
    }

    public IEnumerator CharacterDialogue(string name, string[] lines, float lineDelay)
    {
        foreach (string line in lines)
        {
            uiManager.DisplayNameAndTextbox(name, line);
            yield return new WaitForSeconds(lineDelay);
        }

        uiManager.HideTextbox();
    }

    public IEnumerator Narration(string text, float duration)
    {
        uiManager.DisplayNarration(text);
        yield return new WaitForSeconds(duration);
        uiManager.HideNarration();
    }

    public void HideText()
    {
        uiManager.HideTextbox();
    }
}
