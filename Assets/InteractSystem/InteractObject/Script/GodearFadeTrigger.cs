using TMPro;
using UnityEngine;
using System.Collections;

public class GodearFadeTrigger : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public AudioSource storyAudio;
    public float fadeDuration = 1.5f;
    public float holdDuration = 3f;
    private bool hasTriggered = false;

    void Start()
    {
        // เริ่มด้วยข้อความโปร่งใส
        SetAlpha(0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ShowTextWithFade());
        }
    }

    IEnumerator ShowTextWithFade()
    {
        storyAudio.Play();

        // Fade In
        yield return StartCoroutine(FadeText(0f, 1f, fadeDuration));

        // ค้างไว้ระหว่างเสียงพูด
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        yield return StartCoroutine(FadeText(1f, 0f, fadeDuration));

        Destroy(gameObject); // ลบ Trigger ออก
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        Color color = textMeshPro.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            color.a = alpha;
            textMeshPro.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = endAlpha;
        textMeshPro.color = color;
    }

    void SetAlpha(float a)
    {
        Color color = textMeshPro.color;
        color.a = a;
        textMeshPro.color = color;
    }
}
