using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public Sprite[] backgrounds;
    [TextArea(2, 5)] public string[] dialogueLines;

    public Image imageA;
    public Image imageB;
    public TMP_Text dialogueText;

    public float totalTime = 56f;
    public float fadeTime = 0.5f;
    public float zoomScale = 1.05f;

    int index;
    float switchInterval;
    float timeInSlide;
    bool playing;

    Image current;
    Image next;
    Coroutine transitionRoutine;

    void Start()
    {
        if (backgrounds == null || backgrounds.Length == 0)
            return;

        switchInterval = totalTime / (backgrounds.Length - 1);

        if (fadeTime > switchInterval)
            fadeTime = switchInterval;

        imageA.gameObject.SetActive(true);
        imageB.gameObject.SetActive(true);

        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);

        current = imageA;
        next = imageB;

        index = 0;
        timeInSlide = 0f;
        playing = true;

        current.sprite = backgrounds[0];
        current.rectTransform.localScale = Vector3.one;

        if (dialogueText != null)
        {
            if (dialogueLines != null && dialogueLines.Length > 0)
                dialogueText.text = dialogueLines[0];
            else
                dialogueText.text = "";
        }
    }

    void Update()
    {
        if (!playing)
            return;

        if (index >= backgrounds.Length - 1)
        {
            float tEnd = Mathf.Clamp01(timeInSlide / Mathf.Max(0.0001f, switchInterval));
            current.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * zoomScale, tEnd);
            timeInSlide += Time.deltaTime;
            return;
        }

        timeInSlide += Time.deltaTime;

        float t = Mathf.Clamp01(timeInSlide / Mathf.Max(0.0001f, switchInterval));
        current.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * zoomScale, t);

        if (timeInSlide >= switchInterval)
        {
            timeInSlide -= switchInterval;
            index++;

            if (transitionRoutine != null)
                StopCoroutine(transitionRoutine);

            transitionRoutine = StartCoroutine(TransitionTo(index));
        }
    }

    IEnumerator TransitionTo(int newIndex)
    {
        if (newIndex < 0 || newIndex >= backgrounds.Length)
            yield break;

        next.sprite = backgrounds[newIndex];
        next.rectTransform.localScale = Vector3.one;

        if (dialogueText != null)
        {
            if (dialogueLines != null && newIndex < dialogueLines.Length)
                dialogueText.text = dialogueLines[newIndex];
            else
                dialogueText.text = "";
        }

        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeTime));

            SetAlpha(current, 1f - a);
            SetAlpha(next, a);

            float z = Mathf.Lerp(1f, zoomScale, a);
            next.rectTransform.localScale = Vector3.one * z;

            yield return null;
        }

        SetAlpha(current, 0f);
        SetAlpha(next, 1f);

        Image temp = current;
        current = next;
        next = temp;

        current.rectTransform.localScale = Vector3.one;
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null)
            return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}