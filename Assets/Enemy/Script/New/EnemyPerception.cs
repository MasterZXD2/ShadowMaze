using ShadowMaze.MyCharacterController;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float fearRange = 2f;
    [SerializeField] private float eyeHeightOffset = .5f;
    [SerializeField] private float awarenessRange = 4f;  // ระยะที่ศัตรูจะรู้ว่าผู้เล่นอยู่ใกล้ แม้จะมองไม่เห็น
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private LayerMask groundLayers;

    public LayerMask GroundLayers => groundLayers;
    public float EyeHeightOffset => eyeHeightOffset;
    public float FearRange => fearRange;

    public bool isCanSeePlayer { get; private set; }
    public bool isPlayerInSightRange { get; private set; }
    public bool isPlayerInAwarenessRange { get; private set; }

    public bool isPlayerInAttackRange { get; private set; }
    public bool isPlayerInSightDuringSearch { get; private set; }
    public bool isPlayerInFearRange { get; private set; }

    #region Normal Function
    private void Update()
    {
        PlayerController player = PlayerController.Instance;
        if (!player)
            return;

        UpdateCanSeePlayer(player);

        float dist = Vector3.Distance(transform.position, player.gameObject.transform.position);
        UpdatePlayerInSightRange(dist);
        UpdatePlayerInAwarenessRange(dist);
        UpdatePlayerInAttackRange(dist);
        UpdatePlayerInFearRange(dist);

        UpdatePlayerInSightDuringSearch(player, dist);
    }
    #endregion

    #region Check Function
    private void UpdatePlayerInAwarenessRange(float dist)
    {
        isPlayerInAwarenessRange = CheckWithAwarenessRange(dist);
    }

    private void UpdatePlayerInSightRange(float dist)
    {
        isPlayerInSightRange = dist <= sightRange;
    }
    private void UpdatePlayerInAttackRange(float dist)
    {
        isPlayerInAttackRange = dist <= attackRange;
    }
    private void UpdatePlayerInFearRange(float dist)
    {
        isPlayerInFearRange = dist <= fearRange;
    }

    private void UpdateCanSeePlayer(PlayerController player)
    {
        Vector3 directionToPlayer = (player.gameObject.transform.position - transform.position).normalized;

        // เช็คว่าผู้เล่นอยู่ในมุมมองหรือไม่
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f)
        {
            isCanSeePlayer = false;
            return;
        }

        // ยิง Raycast ตรวจว่าไม่มีอะไรมาบัง
        Vector3 rayOrigin = transform.position + Vector3.up * eyeHeightOffset;
        Vector3 targetPos = player.gameObject.transform.position + Vector3.up * 1.5f; // เผื่อผู้เล่นเตี้ย/สูง
        Vector3 rayDir = (targetPos - rayOrigin).normalized;
        float distance = Vector3.Distance(rayOrigin, targetPos);

        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, distance, ~0))
            isCanSeePlayer = hit.collider.gameObject == player.gameObject;
        else
            isCanSeePlayer = false;
    }
    private void UpdatePlayerInSightDuringSearch(PlayerController player, float dist)
    {
        Vector3 dirToPlayer = (player.gameObject.transform.position - transform.position).normalized;
        float viewAngle = Vector3.Angle(transform.forward, dirToPlayer);
        float maxViewAngle = 100f; // สามารถมองได้ในช่วงมุมประมาณ 100 องศา

        if (viewAngle < maxViewAngle * 0.5f && isPlayerInSightRange
            && !Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, out RaycastHit hit, dist, groundLayers))
        {
            isPlayerInSightDuringSearch = true;
        } else
            isPlayerInSightDuringSearch = false;
    }

    public bool CheckWithAwarenessRange(float dist)
    {
        return dist <= awarenessRange;
    }
    #endregion
}
