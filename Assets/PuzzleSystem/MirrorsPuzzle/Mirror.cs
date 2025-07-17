using ShadowMaze.MyCharacterController;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Mirror : MonoBehaviour
{
    [SerializeField] private Material glass;
    [SerializeField] private Material neon;
    [SerializeField] private LayerMask _mirrorLayerMask;

    private Renderer _renderer;
    private bool b { get => _b; set { 
            _b = value;
            if (_b && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, _mirrorLayerMask))
            {
                if (hit.transform.TryGetComponent<Mirror>(out Mirror mi))
                {
                    mi.HitByFlashlight(transform);
                }
            }
        } }
    private bool _b = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        _renderer.material = b ? neon : glass;
    }

    private void LateUpdate()
    {
        b = false;
    }

    public void HitByFlashlight(Transform lightSource)
    {
        b = CanSeePlayer(lightSource);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position,transform.forward);
    }

    private bool CanSeePlayer(Transform lightSource)
    {
        if (!lightSource) return false;

        Vector3 directionToMirror = (lightSource.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToMirror);

        return angle < 90f;
    }

}

