using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using UnityEngine;

public class TheWatchingEyesAI : MonoBehaviour
{
    [Header("Enemy Config")]

    [Space(10)]
    [Header("Enemy Components")]

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private EnemyState enemyState;
    [SerializeField] private GameObject enemyShadow;

    [Space(10)]
    [Header("Movement")]
    public float EnemyModelRotationSpeed = 10f;

    public float Drag = 0.1f;
    public float MovingThreshold = 0.01f;

    [Header("Walk")]

    public float WalkAcceleration = 0.25f;
    public float WalkSpeed = 4f;

    [SerializeField] private float outSpeed = 5f;
    [SerializeField] private float outRange = .8f;
    [SerializeField] private float attackRange = .5f;
    [SerializeField] private LayerMask _groundlayerMask;

    public bool isMovement { get; private set; }
    public bool Sprint { get; private set; }
    public bool Jump { get; private set; }

    private GameObject _shadow;
    private bool inArea = false;

    private void Start()
    {
        _shadow = Instantiate(enemyShadow,transform);
    }

    private void Update()
    {
        Vector3 or = transform.position + new Vector3(0f, TheWatchingEyesArea.Instance.inGround + .5f, 0f);
        if (Physics.Raycast(or, Vector3.down, out RaycastHit hit, Mathf.Infinity, _groundlayerMask))
        {
            _shadow.transform.position = hit.point + hit.normal * .1f;
        }

        if (!inArea) return;
        if ((or - PlayerController.Instance.transform.position).sqrMagnitude <= outRange * outRange)
        {
            transform.position = Vector3.Lerp(transform.position, or, Time.deltaTime * outSpeed);
        } else
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit0, Mathf.Infinity, _groundlayerMask))
            {
                transform.position = Vector3.Lerp(transform.position, hit0.point + new Vector3(0f, TheWatchingEyesArea.Instance.inGround + .5f, 0f), Time.deltaTime * outSpeed);
            }
        }
        LateralMovement();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0f, TheWatchingEyesArea.Instance.inGround + .5f, 0f),outRange);
    }

    public void EnterArea()
    {
        inArea = true;
    }

    public void ExitArea()
    {
        inArea = false;
    }

    private void LateralMovement()
    {
        //bool isGrounded = _enemyState.InGroundedState();
        Transform player = PlayerController.Instance.transform;

        isMovement = true;
        Vector3 heading = (player.position - transform.position);
        float tarDist = heading.magnitude;
        Vector3 Dir = heading / tarDist;

        Vector3 moveDir = new Vector3(Dir.x, 0, Dir.z);
        RotatePlayerToTar(heading);

        Vector3 moveDelta = moveDir * WalkAcceleration * Time.deltaTime;
        Vector3 newVelo = _characterController.velocity + moveDelta;

        float DragMagnitude = Drag;
        //Add drag to player
        Vector3 currDrag = newVelo.normalized * DragMagnitude * Time.deltaTime;
        newVelo = (newVelo.magnitude > DragMagnitude * Time.deltaTime) ? newVelo - currDrag : Vector3.zero;

        //Move charactor
        newVelo = Vector3.ClampMagnitude(new Vector3(newVelo.x, 0f, newVelo.z), WalkSpeed);
        //newVelo.y += _verticalVelo;
        //newVelo = !isGrounded ? SteepWalls(newVelo) : newVelo;
        _characterController.Move(newVelo * Time.deltaTime);
    }

    private void RotatePlayerToTar(Vector3 dir)
    {
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        targetRotation *= Quaternion.Euler(0f, 90f, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, EnemyModelRotationSpeed * Time.deltaTime);
    }
}