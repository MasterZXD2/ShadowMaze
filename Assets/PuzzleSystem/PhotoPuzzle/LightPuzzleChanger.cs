using UnityEngine;

public class LightPuzzleChanger : Puzzle
{
    [SerializeField] LightPuzzleManager manager;
    public Material grayscaleMat;
    public Material normalMat;

    private Renderer _renderer;
    private float lightHitTimer = 0f;
    [SerializeField] private float timeToChangeMat = 3f;
    [SerializeField] private bool isTheWatchingEyes = false;

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

            Material[] mats = _renderer.materials;
            mats[0] = runtimeMat;
            _renderer.materials = mats;

        }

        manager.RegisterPuzzle(this);
    }

    private void Update()
    {
        if (isChanged) return;

        float progress = Mathf.Clamp01(lightHitTimer / timeToChangeMat);

        Color from = grayscaleMat.color;
        Color to = Color.grey;
        runtimeMat.color = Color.Lerp(from, to, progress);

        if (lightHitTimer >= timeToChangeMat)
        {
            isChanged = true;
            _renderer.material = normalMat;
            transitionProgress = 0f;
            lightHitTimer = 0f;
        }

        /*
        if (isTransitioning && runtimeMat != null && normalMat != null)
        {
            transitionProgress += Time.deltaTime / 1.5f;
            transitionProgress = Mathf.Clamp01(transitionProgress);

            Color from = runtimeMat.color;
            Color to = normalMat.color;
            runtimeMat.color = Color.Lerp(from, to, transitionProgress);

            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                isChanged = true;
                _renderer.material = normalMat;
            }
        }
        */
    }

    public void HitByFlashlight()
    {
        if (isChanged) return;
        lightHitTimer += Time.deltaTime;
    }
}
