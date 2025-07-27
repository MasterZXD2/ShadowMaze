using TMPro;
using UnityEngine;
using System.Collections;

public class FadeText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float fadeDuration = 2f;

    void Start()
    {
        //StartCoroutine(FadeInText());
    }

    IEnumerator FadeInText()
    {
        Color color = textMeshPro.color;
        color.a = 0;
        textMeshPro.color = color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            textMeshPro.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        textMeshPro.color = color;
    }
}
