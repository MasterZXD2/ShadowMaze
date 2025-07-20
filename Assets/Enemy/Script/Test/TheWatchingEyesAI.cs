using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class TheWatchingEyesAI : MonoBehaviour
{
    [Header("Enemy Config")]

    [Space(10)]
    [Header("Enemy Components")]

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private EnemyState enemyState;
    [SerializeField] private GameObject enemyObject;
    [SerializeField] private GameObject enemyShadow;
    [SerializeField] private GameObject shadowWall;

    public float Health = 100f;
    public float MaxHealth = 100f;
    public float AttackDamage = 20f;
    public float AttackCD = 1f;

    [Space(10)]
    [Header("Movement")]
    public float EnemyModelRotationSpeed = 10f;

    public float Drag = 0.1f;
    public float MovingThreshold = 0.01f;

    [Header("Walk")]

    public float WalkAcceleration = 0.25f;
    public float WalkSpeed = 4f;

    public float DashForce = 15f;
    public float DashCD = 3f;
    public float dashDuration = 0.2f;
    [Range(0, 100)] public float DashChange = 50f;

    public List<LightPuzzleChanger> lightPuzzles = new List<LightPuzzleChanger>();

    [Range(0, 100)] public float DarkScreenChange = 50f;
    [Range(0, 100)] public float WallShadowChange = 50f;
    [Range(0, 100)] public float StayChange = 50f;

    [SerializeField] private float stayTime = 1f;
    [SerializeField] private float outSpeed = 5f;
    [SerializeField] private float outRange = .8f;
    [SerializeField] private float attackRange = .5f;
    [SerializeField] private float inGround = 0f;
    [SerializeField] private LayerMask _groundlayerMask;
    public bool isDash {  get; private set; }
    public bool isAttack { get; private set; }
    public bool isMovement { get; private set; }
    public bool Sprint { get; private set; }
    public bool Jump { get; private set; }

    private float dashTimeRemaining = 0f;
    private bool canDash = true;
    private Vector3 dashDir;

    private GameObject _shadow;
    private Renderer _rendererSha;
    private bool inArea = false;
    private int finishs = 0;
    private bool staying = false;

    private void Start()
    {
        _shadow = Instantiate(enemyShadow,transform);
        _rendererSha = _shadow.GetComponent<Renderer>();
        foreach (LightPuzzleChanger changer in lightPuzzles)
        {
            changer.setToBossTWE(this);
        }
    }

    private void Update()
    {
        foreach (LightPuzzleChanger changer in lightPuzzles)
        {
            if (changer.isFinish) finishs++;
        }

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, _groundlayerMask))
        {
            _shadow.transform.position = hit.point + hit.normal * .1f;
        }

        if(isDash)
        {
            // พุ่งไปในทิศทาง dash
            _characterController.Move(dashDir * DashForce * Time.deltaTime);
            dashTimeRemaining -= Time.deltaTime;

            if (dashTimeRemaining <= 0f)
            {
                isDash = false;
            }

            return;
        }

        if (!inArea) return;
        Transform player = PlayerController.Instance.transform;

        Vector3 pos = transform.position;
        float dist = (transform.position - player.position).sqrMagnitude;
        if (dist <= outRange * outRange)
        {
            _rendererSha.enabled = false;
            pos = Vector3.Lerp(enemyObject.transform.position, transform.position, Time.deltaTime * outSpeed);
            if (dist <= attackRange * attackRange) AttackPlayer();
        } else
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit0, Mathf.Infinity, _groundlayerMask))
            {
                _rendererSha.enabled = true;
                pos = Vector3.Lerp(enemyObject.transform.position, hit0.point - new Vector3(0f, inGround + .5f, 0f), Time.deltaTime * outSpeed);
            }
        }
        enemyObject.transform.position = pos;

        if (finishs >= 1)
        {
            if (Random.Range(0f, 100f) < StayChange) Stay();
            if (Random.Range(0f,100f) < DarkScreenChange)
            {
                PlayerController.Instance.MakeScreenDark();
            }
            if (Random.Range(0f,100f) < WallShadowChange)
            {
                GameObject wall = Instantiate(shadowWall, transform.parent);

                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                float distToPlayer = Vector3.Distance(transform.position, player.position) + 1.2f;

                // กำหนดตำแหน่งให้เริ่มกลางระหว่างศัตรูกับผู้เล่น
                wall.transform.position = transform.position + dirToPlayer * (distToPlayer * 0.5f);

                // หมุนกำแพงให้หันไปทางผู้เล่น
                wall.transform.rotation = Quaternion.LookRotation(dirToPlayer);

                // ล็อคไม่ให้เอียง (ถ้าใช้ LookRotation แล้วมันเอียง)
                Vector3 euler = wall.transform.eulerAngles;
                wall.transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

                // ปรับขนาดกำแพงให้ยาวพอ
                wall.transform.localScale = new Vector3(0.2f, 20f, distToPlayer);
            }
        }
        if (finishs >= 2)
        {

        }
        if (finishs >= 3)
        {

        }

        if (!staying) LateralMovement();
        finishs = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,outRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,attackRange);
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

        if (Random.Range(0f, 100f) < DashChange && canDash)
        {
            canDash = false;
            isDash = true;
            dashTimeRemaining = dashDuration;

            dashDir = heading.normalized;
            Invoke(nameof(ResetDash), DashCD);
        }
    }

    private void RotatePlayerToTar(Vector3 dir)
    {
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        targetRotation *= Quaternion.Euler(0f, 90f, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, EnemyModelRotationSpeed * Time.deltaTime);
    }

    private void AttackPlayer()
    {
        if (!isAttack)
        {
            PlayerController.Instance._playerStats.Sanity -= AttackDamage;
            isAttack = true;
            Invoke(nameof(ResetAtteck), AttackCD);
        }
    }

    private void Stay()
    {
        staying = true;
        Invoke(nameof(ResetStay), stayTime);
    }

    private void ResetStay() => staying = false;
    private void ResetAtteck() => isAttack = false;
    private void ResetDash() => canDash = true;
}