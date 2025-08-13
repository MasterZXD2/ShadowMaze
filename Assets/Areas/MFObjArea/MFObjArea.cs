using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public struct rendObj
{
    public Renderer renderer;
    public List<Color> colors;
    public List<Material> materials;
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
            
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(mats[i]);
            }
            r.materials = mats;

            // เก็บข้อมูลเดิม
            rendObj ro = new rendObj
            {
                renderer = r,
                colors = new List<Color>(),
                materials = new List<Material>()
            };

            for (int i = 0; i < r.materials.Length; i++)
            {
                ro.colors.Add(r.materials[i].color);
                ro.materials.Add(r.materials[i]);
            }

            rendObjs.Add(ro);

            // ตั้งค่าโปร่งใส
            for (int i = 0; i < r.materials.Length; i++)
            {
                Color _c = r.materials[i].color;
                _c.a = 0f;
                r.materials[i].color = _c;
            }
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

    IEnumerator Fade(rendObj obj, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);

            for (int i = 0; i < obj.colors.Count; i++)
            {
                Color newColor = obj.colors[i];
                newColor.a = alpha;
                obj.materials[i].color = newColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final alpha is exactly endAlpha
        for (int i = 0; i < obj.colors.Count; i++)
        {
            Color finalColor = obj.colors[i];
            finalColor.a = endAlpha;
            obj.materials[i].color = finalColor;
        }
    }

    IEnumerator FadeIn(rendObj obj)
    {
        yield return Fade(obj, 0f, 1f, fadeDuration);
    }

    IEnumerator FadeOut(rendObj obj)
    {
        yield return Fade(obj, 1f, 0f, fadeDuration);
    }
}
