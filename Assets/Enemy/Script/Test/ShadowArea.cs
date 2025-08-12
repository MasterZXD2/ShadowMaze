using UnityEngine;

public class ShadowArea : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float dSpawn;
    private float timer = 0;
    private bool _spawn = false;

    private void Update()
    {
        if (_spawn) return;
        timer += Time.deltaTime;
        if (timer > dSpawn)
        {
            _spawn = true;
            GameObject g = Instantiate(enemy);
            g.transform.position = transform.position + offset;
        }
    }
}
