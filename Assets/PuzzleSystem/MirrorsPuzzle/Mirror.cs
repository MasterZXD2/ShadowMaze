using UnityEngine;

public class Mirror : MonoBehaviour
{
    [SerializeField] private Material glass;
    [SerializeField] private Material neon;
    private bool b = false;

    private void Update()
    {
        Debug.Log(b);
    }

    private void LateUpdate()
    {
        b = false;
    }

    public void HitByFlashlight()
    {
        b = true;
    }
}
