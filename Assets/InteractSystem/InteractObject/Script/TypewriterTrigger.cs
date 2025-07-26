using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterTrigger : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public AudioSource storyAudio;
    public string message = "ยินดีต้อนรับสู่เงามืด...";
    public float delayPerChar = 0.05f;
    public float holdDuration = 2f;
    public float fadeDuration = 1.5f;
    private bool hasTriggered = false;

    void Start()
    {
        textMeshPro.text = "";
        SetAlpha(0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayTypewriterSequence());
        }
    }

    IEnumerator PlayTypewriterSequence()
    {
        storyAudio.Play();
        yield return StartCoroutine(FadeText(0f, 1f, fadeDuration));
        yield return StartCoroutine(TypeText(message));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeText(1f, 0f, fadeDuration));
        gameObject.SetActive(false); // หรือ Destroy(gameObject);
    }

    IEnumerator TypeText(string msg)
    {
        textMeshPro.text = "";
        foreach (char c in msg)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(delayPerChar);
        }
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            SetAlpha(alpha);
            time += Time.deltaTime;
            yield return null;
        }
        SetAlpha(endAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color color = textMeshPro.color;
        color.a = alpha;
        textMeshPro.color = color;
    }
}
