using ShadowMaze.CutSceneSystem;
using ShadowMaze.MyCharacterController;
using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class GodearFadeTrigger : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public AudioSource storyAudio;
    public CutScene cutScene;
    public float fadeDuration = 1.5f;
    public float holdDuration = 3f;
    private bool hasTriggered = false;
    private bool holdf = false;
    private bool playf = false;

    void Start()
    {
        // เริ่มด้วยข้อความโปร่งใส
        SetAlpha(0f);

        cutScene.SetVideoCamera(PlayerController.Instance._plrCamera);

        cutScene.Prepare();
        cutScene.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(Play());
        }
    }

    IEnumerator Play() {
        StartCoroutine(ShowTextWithFade());
        cutScene.Play();
        yield return new WaitUntil(() => !cutScene.IsPlaying && holdf);
        Destroy(gameObject);
    }

    IEnumerator ShowTextWithFade()
    {
        storyAudio.Play();
        textMeshPro.gameObject.SetActive(true);
        // Fade In
        yield return StartCoroutine(FadeText(0f, 1f, fadeDuration));

        // ค้างไว้ระหว่างเสียงพูด
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        yield return StartCoroutine(FadeText(1f, 0f, fadeDuration));

        holdf = true;
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
