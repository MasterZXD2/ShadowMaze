using ShadowMaze.MyCharacterController;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Mirror : MonoBehaviour
{
    [SerializeField] private Material glass;
    [SerializeField] private Material neon;
    [SerializeField] private LayerMask _mirrorLayerMask;

    private Renderer _renderer;
    private bool b = false;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        Material[] mats = _renderer.materials;
        if (b) {
            mats[0] = neon;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f, _mirrorLayerMask))
            {
                if (hit.transform.TryGetComponent<Mirror>(out Mirror mi))
                {
                    mi.HitByFlashlight(transform);
                }
            }
        }
        else mats[0] = glass;
        _renderer.materials = mats;
    }

    private void LateUpdate()
    {
        b = false;
    }

    public void HitByFlashlight(Transform lightSource)
    {
        b = CanSeePlayer(lightSource);
        Debug.Log(b);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position,transform.forward);
    }

    private bool CanSeePlayer(Transform lightSource)
    {
        if (!lightSource) return false;

        Vector3 directionToMirror = (transform.position - lightSource.position).normalized;
        float angle = Vector3.Angle(lightSource.forward, directionToMirror);

        return angle < 45f; // องศาที่มองเห็นกระจก เช่น 90 องศาครึ่ง
    }

}

