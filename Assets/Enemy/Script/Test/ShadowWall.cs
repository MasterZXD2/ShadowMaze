using UnityEngine;

public class ShadowWall : MonoBehaviour
{
    [SerializeField] private float DestoryTime = 1f;
    private float _destorytimer;
    void Start()
    {
        _destorytimer = DestoryTime;
    }

    void Update()
    {
        _destorytimer -= Time.deltaTime;
        if (_destorytimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
