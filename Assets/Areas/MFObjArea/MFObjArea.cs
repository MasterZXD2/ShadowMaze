using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct rendObj
{
    public Renderer renderer;
    public Color color;
}

public class MFObjArea : MonoBehaviour
{
    [field: SerializeField] private List<GameObject> mObjects;
    [SerializeField] private float fadeDuration;
    private List<rendObj> rendObjs = new List<rendObj>();

    private void Start()
    {
        foreach (GameObject item in mObjects)
        {
            Renderer r = item.GetComponent<Renderer>();
            Color c = r.material.color;
            rendObjs.Add(new rendObj { renderer = r, color = c });

            Color _c = c;
            _c.a = 0f;
            r.material.color = _c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foreach (rendObj obj in rendObjs)
        {
            StartCoroutine(FadeIn(obj));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foreach (rendObj obj in rendObjs)
        {
            StartCoroutine(FadeOut(obj));
        }
    }

    IEnumerator FadeIn(rendObj obj)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            Color newColor = obj.color;
            newColor.a = alpha;
            obj.renderer.material.color = newColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ให้มั่นใจว่า alpha = 1 สุดท้าย
        Color finalColor = obj.color;
        finalColor.a = 1f;
        obj.renderer.material.color = finalColor;
    }

    IEnumerator FadeOut(rendObj obj)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Color newColor = obj.color;
            newColor.a = alpha;
            obj.renderer.material.color = newColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ให้มั่นใจว่า alpha = 1 สุดท้าย
        Color finalColor = obj.color;
        finalColor.a = 0f;
        obj.renderer.material.color = finalColor;
    }
}
