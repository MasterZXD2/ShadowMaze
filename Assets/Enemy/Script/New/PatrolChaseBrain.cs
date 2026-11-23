using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using UnityEngine;

public class PatrolChaseBrain : EnemyBrain
{
    public enum Mode { Patrol, Chase, Investigate, IdleLookAround, ActiveSearch }
    public Mode CurrentMode { get; private set; } = Mode.Patrol;

    [SerializeField] protected EnemyPresentation presentation;
    [SerializeField] private float minSearchDuration = 1f;
    [SerializeField] private float maxSearchDuration = 3f;
    [SerializeField] private float walkPointRange = 12f;
    public Vector3 lastPlayerPosition { get; protected set; }
    public Vector3 lastGoToPosition { get; protected set; }
    public bool isMovement { get; protected set; } = false;
    public bool hasLastPlayerPosition { get; protected set; } = false;
    
    private Vector3 goToPosition;
    protected bool hasPatrolDestination;
    private float searchTimer = 0f;
    private float searchDuration = 0f;
    private float sameGoToPositionTimer = 0f;
    protected bool isBlind = false;
    private bool hasJumpedSinceCompletePath;
    private bool hasLastJumpTarget;
    private Vector3 lastJumpTarget;

    private void Start()
    {
        combat.attackSucceeded.AddListener(enemy.Die);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, perception.AwarenessRange);
        Gizmos.DrawWireSphere(lastPlayerPosition, perception.AwarenessRange);

        if (hasLastPlayerPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastPlayerPosition, 0.2f);
            Gizmos.DrawLine(transform.position, lastPlayerPosition);
        }
    }

    protected override void NormalMove()
    {
        var player = PlayerController.Instance;
        if (!player)
            return;

        if (CurrentMode == Mode.IdleLookAround || CurrentMode == Mode.ActiveSearch)
        {
            SearchRotate();
            return;
        }

        bool canSee = !isBlind && perception.isPlayerInSightRange && perception.isCanSeePlayer;
        bool canTrack = !isBlind && hasLastPlayerPosition && perception.isPlayerInSightRange
            && perception.isPlayerInAwarenessRange;
        if (canSee || canTrack)
        {
            RememberPlayer(player);
            CurrentMode = Mode.Chase;
            hasPatrolDestination = false;
        }
        else if (CurrentMode == Mode.Chase)
        {
            CurrentMode = Mode.Investigate;
        }

        if (motor.HasCompletePath)
            hasJumpedSinceCompletePath = false;

        // Try one jump per partial path; wait until navigation finds a complete route before retrying.
        if (!hasJumpedSinceCompletePath && motor.HasIncompletePath && (hasLastPlayerPosition || hasPatrolDestination)
            && TryStartJump(hasLastPlayerPosition ? lastPlayerPosition : goToPosition))
        {
            return;
        }

        if (sameGoToPositionTimer > 1.2f && isMovement)
        {
            Vector3 point;
            if (!hasJumpedSinceCompletePath)
            {
                if (GetGroundAtAngle(transform.position + Vector3.up * perception.EyeHeightOffset, out point))
                {
                    if (TryStartJump(point))
                        return;
                }
                else
                {
                    if (isBlind)
                        return;

                    sameGoToPositionTimer = 0f;
                    hasPatrolDestination = false;
                    isBlind = true;
                    Invoke(nameof(UnBlind), 2f);
                    return;
                }
            }
        }

        if (CurrentMode == Mode.Chase)
        {
            if (canSee && perception.isPlayerInAttackRange)
                combat.AttackPlayer();
            else
                ChasePlayer();
        }
        else
            Patroling();

        if (transform.position == lastGoToPosition) sameGoToPositionTimer += Time.deltaTime;
        else sameGoToPositionTimer = 0f;

        lastGoToPosition = transform.position;
    }

    private void RememberPlayer(PlayerController player)
    {
        // Store a navigation point at foot height, not a point at eye/chest height.
        Vector3 position = player.transform.position;
        if (player._characterController)
            position.y = player._characterController.bounds.min.y;
        lastPlayerPosition = position;
        hasLastPlayerPosition = true;
    }

    protected void ChasePlayer()
    {
        CurrentMode = Mode.Chase;
        state.MovementSet(EnemyMovementState.Walking);
        motor.SetDestination(lastPlayerPosition);
        isMovement = true;
    }

    private void Patroling()
    {
        bool investigating = CurrentMode == Mode.Investigate;
        if (!investigating && !hasPatrolDestination)
        {
            SearchWalkPoint();
            if (!hasPatrolDestination)
            {
                isMovement = false;
                state.MovementSet(EnemyMovementState.Idling);
                motor.SetDestination(transform.position);
                return;
            }
        }

        Vector3 destination = investigating ? lastPlayerPosition : goToPosition;
        Vector3 toDestination = transform.position - destination;
        toDestination.y = 0f;
        if (toDestination.sqrMagnitude < 1f)
        {
            StartSearch(investigating ? Mode.ActiveSearch : Mode.IdleLookAround);
            return;
        }

        state.MovementSet(EnemyMovementState.Walking);
        motor.SetDestination(destination);
        isMovement = true;
    }

    private void SearchRotate()
    {
        if (perception.isPlayerInSightDuringSearch)
        {
            RememberPlayer(PlayerController.Instance);
            ChasePlayer();
            return;
        }

        presentation.SearchRotate();

        searchTimer += Time.deltaTime;
        if (searchTimer >= searchDuration)
        {
            hasLastPlayerPosition = false;
            CurrentMode = Mode.Patrol;
        }
    }

    private void StartSearch(Mode mode)
    {
        CurrentMode = mode;
        hasPatrolDestination = false;
        isMovement = false;
        sameGoToPositionTimer = 0f;
        searchTimer = 0f;
        searchDuration = UnityEngine.Random.Range(minSearchDuration, maxSearchDuration);
        if (mode == Mode.ActiveSearch)
        {
            searchDuration += 1f;
            presentation.StartActiveSearch();
        }
        else
            presentation.StartIdleLookAround();

        state.MovementSet(EnemyMovementState.Idling);
        motor.SetDestination(transform.position);
    }

    private void SearchWalkPoint()
    {
        float rZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float rX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        Vector3 curr = transform.position;
        goToPosition = new Vector3(curr.x + rX, curr.y, curr.z + rZ);

        if (Physics.Raycast(goToPosition + Vector3.up, Vector3.down, out var hit, 3f, perception.GroundLayers,
            QueryTriggerInteraction.Ignore))
        {
            goToPosition = hit.point;
            hasPatrolDestination = true;
        }
    }

    private void UnBlind() => isBlind = false;

    private bool TryStartJump(Vector3 worldTarget)
    {
        if (hasLastJumpTarget && (worldTarget - lastJumpTarget).sqrMagnitude < 1f)
            return false;
        if (!motor.TryJumpTo(worldTarget, stats))
            return false;

        lastJumpTarget = worldTarget;
        hasLastJumpTarget = true;
        hasJumpedSinceCompletePath = true;
        sameGoToPositionTimer = 0f;
        isMovement = false;
        return true;
    }

    public bool GetGroundAtAngle(Vector3 origin, out Vector3 hitTopPoint)
    {
        hitTopPoint = Vector3.zero;

        // ยิงลงแบบทำมุม 45 องศา (ด้านหน้า + ลง)
        Vector3 forward = transform.forward;
        Vector3 down = Vector3.down;

        // สร้างทิศทาง 45 องศา: ใช้วิธี normalize หรือ lerp
        Vector3 direction = (forward + down).normalized;

        // ระยะยิง
        float rayDistance = 5f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, perception.GroundLayers))
        {
            // hit.point คือจุดชน
            // hit.normal คือ normal ของพื้น

            // ปรับตำแหน่งให้ขึ้นบนสุดของพื้น (เช่นเอาไว้ยืน)
            hitTopPoint = hit.point + hit.normal * 0.1f; // ดันออกเล็กน้อยเพื่อไม่จม
            return true;
        }

        return false;
    }
}
