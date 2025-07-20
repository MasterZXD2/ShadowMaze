using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Mirror : InteractObject
{
    [SerializeField] private Material glass;
    [SerializeField] private Material neon;
    [SerializeField] private LayerMask _mirrorLayerMask;
    [SerializeField] private bool canMove = false;
    [SerializeField] private float rotationSpeed = 5;

    private Renderer _renderer;
    private bool b
    {
        get => _b; set
        {
            _b = value;
            if (_b && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, _mirrorLayerMask))
            {
                if (hit.transform.TryGetComponent<Mirror>(out Mirror mi))
                {
                    mi.HitByFlashlight(transform);
                }
                if (hit.transform.TryGetComponent<LightPuzzleChanger>(out LightPuzzleChanger lpc))
                {
                    lpc.HitByFlashlight();
                }
            }
        }
    }
    private bool _b = false;
    private Quaternion _rotation;

    protected override void Start()
    {
        _rotation = transform.rotation;
        base.Start();
        _renderer = GetComponent<Renderer>();
    }

    protected override void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, _rotation, rotationSpeed * Time.deltaTime);
        base.Update();
        _renderer.material = b ? neon : glass;
        b = false;
    }

    public void HitByFlashlight(Transform lightSource)
    {
        b = CanSeePlayer(lightSource);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position,transform.forward);
        Gizmos.color = Color.black;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, _mirrorLayerMask))
        {
            Gizmos.DrawLine(transform.position,hit.point);
            Gizmos.DrawSphere(hit.point, .2f);
        }
    }

    private bool CanSeePlayer(Transform lightSource)
    {
        if (!lightSource) return false;

        Vector3 directionToMirror = (lightSource.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToMirror);

        return angle < 90f;
    }

    public override void Interact()
    {
        if (!canMove) return;
        _rotation = transform.rotation * Quaternion.Euler(0f,5f,0f);
    }
}

