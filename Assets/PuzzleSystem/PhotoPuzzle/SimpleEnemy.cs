using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public Material grayscaleMat;
    public Material normalMat;

    private Renderer _renderer;
    private float lightHitTimer = 0f;
    private float timeToChangeMat = 3f;

    private bool isChanged = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null && grayscaleMat != null)
        {
            _renderer.material = grayscaleMat;
        }
    }

    public void OnHitByLight(float deltaTime)
    {
        if (isChanged) return;

        lightHitTimer += deltaTime;
        if (lightHitTimer >= timeToChangeMat)
        {
            if (_renderer != null && normalMat != null)
            {
                _renderer.material = normalMat;
                isChanged = true;
            }
        }
    }

    public void ResetLightHit() => lightHitTimer = 0f;
}
