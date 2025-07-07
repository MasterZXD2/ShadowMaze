using UnityEngine;

public class LightPuzzleChanger : MonoBehaviour
{
    public Material grayscaleMat;
    public Material normalMat;

    private Renderer _renderer;
    private float lightHitTimer = 0f;
    [SerializeField] private float timeToChangeMat = 3f;

    private bool isChanged = false;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    private Material runtimeMat;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null && grayscaleMat != null)
        {
            runtimeMat = new Material(grayscaleMat);
            _renderer.material = grayscaleMat;
        }

        PuzzleManager.Instance?.RegisterPuzzle(this);
    }

    private void Update()
    {
        if (isTransitioning && runtimeMat != null && normalMat != null)
        {
            transitionProgress += Time.deltaTime / 1.5f;
            transitionProgress = Mathf.Clamp01(transitionProgress);

            Color from = grayscaleMat.color;
            Color to = normalMat.color;
            runtimeMat.color = Color.Lerp(from, to, transitionProgress);

            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                isChanged = true;
                _renderer.material = normalMat;
            }
        }
    }

    public void HitByFlashlight()
    {
        if (isChanged) return;

        lightHitTimer += Time.deltaTime;
        if (lightHitTimer >= timeToChangeMat)
        {
            if (_renderer != null && normalMat != null)
            {
                _renderer.material = normalMat;
                isChanged = true;
            }
        }
    }
}
