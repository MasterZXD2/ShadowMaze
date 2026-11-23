using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public enum Motor
{
    NevMashAgent,
    CharacterController
}

public class EnemyMotor : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Motor motor = Motor.NevMashAgent;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController characterController;

    public bool IsStop { get; private set; }
    public bool IsJumping { get; private set; }
    public NavMeshAgent Agent => agent;
    private bool CanNavigate => !IsStop && !IsJumping && agent && agent.isActiveAndEnabled && agent.isOnNavMesh;
    public bool HasIncompletePath => CanNavigate && !agent.pathPending && agent.hasPath
        && agent.pathStatus != NavMeshPathStatus.PathComplete;
    public bool HasCompletePath => CanNavigate && !agent.pathPending && agent.hasPath
        && agent.pathStatus == NavMeshPathStatus.PathComplete;

    private Vector3 takeoffPosition, jumpDirection;
    private float jumpDistance, elapsed, landingDeadline;
    private float jumpSpeed, gravity, walkSpeed, acceleration;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!characterController) characterController = GetComponent<CharacterController>();
        ApplyMode();
    }

    private void OnDrawGizmos()
    {
        if (CanNavigate)
        {
            Gizmos.color = Color.black;
            var agentPath = agent.path;
            Vector3 prevConrner = transform.position;
            foreach (var c in agentPath.corners)
            {
                Gizmos.DrawLine(prevConrner, c);
                Gizmos.DrawSphere(c, 0.1f);
                prevConrner = c;
            }
        }
    }

    private void ApplyMode()
    {
        if (agent) agent.enabled = !IsStop && motor == Motor.NevMashAgent;
        if (characterController) characterController.enabled = !IsStop && motor == Motor.CharacterController;
    }

    public void ChangeMotor(Motor mode)
    {
        if (IsJumping) return;
        motor = mode;
        ApplyMode();
    }

    public void SetDestination(Vector3 worldPosition)
    {
        if (CanNavigate) agent.SetDestination(worldPosition);
    }

    // A displacement for this frame, never an absolute destination.
    private CollisionFlags MoveBy(Vector3 displacement) => characterController.Move(displacement);

    // Distance travelled while accelerating up to walking speed.
    private static float HorizontalDistance(float time, float speed, float accel)
    {
        float rampTime = Mathf.Min(time, speed / accel);
        return 0.5f * accel * rampTime * rampTime + speed * (time - rampTime);
    }

    public bool TryJumpTo(Vector3 worldTarget, EnemyStats stats)
    {
        if (!isActiveAndEnabled || !CanNavigate || !characterController || !stats
            || stats.Gravity <= 0f || stats.JumpSpeed <= 0f || stats.WalkSpeed <= 0f || stats.InAirAcceleration <= 0f)
            return false;

        Vector3 offset = worldTarget - transform.position;
        float verticalTerm = stats.JumpSpeed * stats.JumpSpeed - 2f * stats.Gravity * offset.y;
        if (verticalTerm < 0f) return false;
        float flightTime = (stats.JumpSpeed + Mathf.Sqrt(verticalTerm)) / stats.Gravity;
        Vector3 horizontal = new Vector3(offset.x, 0f, offset.z);
        float distance = horizontal.magnitude;
        if (distance > HorizontalDistance(flightTime, stats.WalkSpeed, stats.InAirAcceleration)) return false;

        takeoffPosition = transform.position;
        jumpDirection = horizontal.normalized;
        jumpDistance = distance;
        jumpSpeed = stats.JumpSpeed;
        gravity = stats.Gravity;
        walkSpeed = stats.WalkSpeed;
        acceleration = stats.InAirAcceleration;
        elapsed = 0f;
        landingDeadline = flightTime + 2f; // Recover if the target has no floor collider.

        IsJumping = true;
        motor = Motor.CharacterController;
        ApplyMode();
        return true;
    }

    private void Update()
    {
        if (!IsJumping || IsStop || Time.deltaTime <= 0f) return;

        float next = elapsed + Time.deltaTime;
        float previousDistance = Mathf.Min(jumpDistance, HorizontalDistance(elapsed, walkSpeed, acceleration));
        float nextDistance = Mathf.Min(jumpDistance, HorizontalDistance(next, walkSpeed, acceleration));
        float dy = (jumpSpeed - 0.5f * gravity * (elapsed + next)) * Time.deltaTime;
        CollisionFlags contact = MoveBy(jumpDirection * (nextDistance - previousDistance) + Vector3.up * dy);
        elapsed = next;

        if (dy <= 0f && (contact & CollisionFlags.Below) != 0) FinishJump(true);
        else if (elapsed >= landingDeadline) FinishJump(false);
    }

    private void FinishJump(bool grounded)
    {
        // Return to the takeoff point if the landing is outside this agent's NavMesh.
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        Vector3 position = takeoffPosition;
        if (grounded && NavMesh.SamplePosition(transform.position, out var hit, Mathf.Max(agent.height, agent.radius), filter))
            position = hit.position;

        characterController.enabled = false;
        transform.position = position;
        IsJumping = false;
        motor = Motor.NevMashAgent;
        ApplyMode();
        if (!agent.Warp(position))
            Debug.LogWarning("Could not return the enemy to its NavMesh after jumping.", this);
    }

    public void Warp(Vector3 position)
    {
        if (CanNavigate) agent.Warp(position);
    }

    public bool IsGrounded() => characterController && characterController.enabled && characterController.isGrounded;

    public float GetColliderHeight() { return characterController ? characterController.height : .5f; }

    public void Pause()
    {
        IsStop = true;
        ApplyMode();
    }

    public void Resume()
    {
        IsStop = false;
        ApplyMode();
    }
}
