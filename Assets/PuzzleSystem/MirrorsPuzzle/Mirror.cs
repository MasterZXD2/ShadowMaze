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

    private bool _rotate = false;
    private TheWatchingEyesAI boss;
    private Renderer _renderer;
    private Mirror m;
    private Mirror pm;
    private bool b
    {
        get => _b; set
        {
            _b = value;
            if (_b && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, _mirrorLayerMask))
            {
                if (hit.transform.TryGetComponent<Mirror>(out Mirror mi))
                {
                    if (pm != null) if (pm == m) return;
                    m = mi;
                    mi.HitByFlashlight(transform, false);
                }
                if (hit.transform.TryGetComponent<LightPuzzleChanger>(out LightPuzzleChanger lpc))
                {
                    lpc.HitByFlashlight();
                }
            } else
            {
                pm = null;
                m = null;
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
        base.Update();
        if (!_playerIsInRange && _rotate) _rotate = false;
        if (_rotate)
        {
            Transform rp = PlayerController.Instance.transform;

            Vector3 dir = (rp.position - transform.position);
            dir.y = 0f; // หันเฉพาะแนว Y
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        _renderer.material = b ? neon : glass;
        b = false;
    }

    public void HitByFlashlight(Transform lightSource,bool bi)
    {
        if (!bi)
        {
            if (lightSource.TryGetComponent<Mirror>(out Mirror mi))
            {
                pm = mi;
            }
        }
        b = CanSeePlayer(lightSource,bi);
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

    private bool CanSeePlayer(Transform lightSource,bool bi)
    {
        if (!lightSource) return false;
        if (canMove && bi) return false;

        Vector3 directionToMirror = (lightSource.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToMirror);

        return angle < 90f;
    }

    public override void Interact()
    {
        if (!canMove) return;
        if (!_rotate && boss) boss.MirrorInteract(this);
        _rotate = !_rotate;
    }

    public void setToBossTWE(TheWatchingEyesAI twe)
    {
        boss = twe;
    }
}

