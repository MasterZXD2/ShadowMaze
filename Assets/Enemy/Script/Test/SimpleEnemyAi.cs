using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace ShadowMaze.EnemyCharactor
{
    public class SimpleEnemyAi : MonoBehaviour
    {
        #region Class Veriables
        [Header("Enemy Config")]

        [Space(10)]
        [Header("Enemy Components")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private EnemyState enemyState;

        [Space(10)]
        [Header("Stats")]
        public float Health = 100f;
        public float MaxHealth = 100f;

        [Space(10)]
        [Header("Attack")]
        [SerializeField] private float attackRange;
        [SerializeField] private float AttackCD;
        [SerializeField] private float AttackDamage;

        [Space(10)]
        [Header("Move")]
        [SerializeField] private float walkPointRange;
        [SerializeField] private float EnemyModelRotationSpeed = 10f;

        [Header("Sight Settings")]
        [SerializeField] private float sightRange;
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private float eyeHeightOffset = 1.5f;

        [Space(10)]
        [Header("Search Settings")]
        [SerializeField] private float minSearchDuration = 1f;
        [SerializeField] private float maxSearchDuration = 3f;
        [SerializeField] private float rotationAngleLimit = 60f;
        [SerializeField] private float activeRotationAngleLimit = 90f;
        [SerializeField] private float rotationSpeedDuringSearch = 60f;
        [SerializeField] private float activeRotationSpeedDuringSearch = 120f;

        [Space(10)]
        [Header("Awareness")]
        [SerializeField] private float awarenessDistance = 3.5f; // ระยะที่ศัตรูจะรู้ว่าผู้เล่นอยู่ใกล้ แม้จะมองไม่เห็น

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;
        
        [Space(10)]
        [Header("Transformation Settings")]
        [SerializeField] private float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง (ตั้งเป็น 0 จะเปลี่ยนทันที)
        [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
        [SerializeField] private MeshRenderer enemyMeshRenderer; // MeshRenderer ของแคปซูล (ศัตรู)
        [SerializeField] private CharacterController enemyController; // Collider ของแคปซูล (ศัตรู)
        [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง

        public Action hit { get; private set; }
        public bool isMovement { get; private set; }

        private bool isSearching = false;
        private float searchTimer = 0f;
        private float searchDuration = 0f;
        private float currRotationAngleLimit = 60f;
        private float currRotationSpeedDuringSearch = 60f;
        private Quaternion baseRotation;
        private float currentRotationOffset = 0f; // หมุนสะสม
        private int rotationDirection = 1; // 1 = ขวา, -1 = ซ้าย

        private bool isPlayerInSight, isPlayerInAttack;
        private Transform _player;
        private Vector3 _GTPosition;
        private bool _positionSet;
        private bool isAttack;
        private Vector3 _lastPlayerPosition;
        private bool isLPP = false;
        private bool isPH = false;

        private bool astop = false;

        private GameObject currentCubeFormInstance; // ตัวแปรเก็บ Instance ของกล่องที่ถูกสร้างขึ้นมา
        private bool isCurrentlyHitByFlashlight = false; // สถานะปัจจุบันว่ากำลังโดนไฟฉายส่องหรือไม่
        private float flashlightHitTimer = 0f; // นับเวลาที่โดนไฟฉายส่อง (เพื่อเปลี่ยนเป็นกล่อง)
        private float cubeFormTimer = 0f; // นับเวลาที่อยู่ในร่างกล่อง (เพื่อกลับร่างเดิม)

        private EnemyMovementState _lastMovementState = EnemyMovementState.Falling;
        #endregion

        #region Normal Function
        private void Awake() => MaxHealth = Health;

        private void Start()
        {
            hit = HitByFlashlight;
            _player = PlayerController.Instance.gameObject.transform;
            if (!agent) agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (!_player) return;

            if (isSearching)
            {
                SearchRotate();
                return;
            }

            if (enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
            {
                if (isCurrentlyHitByFlashlight)
                {
                    cubeFormTimer = 0f;
                }
                else
                {
                    cubeFormTimer += Time.deltaTime;
                    if (cubeFormTimer >= cubeFormDuration)
                    {
                        TransitionToEnemyForm();
                        cubeFormTimer = 0f;
                    }
                }
            } else
            {
                if (astop) return;
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist <= sightRange)
                {
                    if (CanSeePlayer())
                    {
                        _lastPlayerPosition = _player.position;
                        _lastPlayerPosition.y += PlayerController.Instance._characterController.height / 2f;

                        if (dist <= attackRange)
                        {
                            AttackPlayer();
                        }
                        else
                        {
                            ChasePlayer();
                        }
                    }
                    else if (isLPP && IsPlayerInAwarenessRange())
                    {
                        _lastPlayerPosition = _player.position;
                        _lastPlayerPosition.y += PlayerController.Instance._characterController.height / 2f;
                        ChasePlayer();
                    }
                    else
                    {
                        Patroling();
                        flashlightHitTimer = 0f;
                        return;
                    }
                }
                else
                {
                    Patroling();
                    flashlightHitTimer = 0f;
                    return;
                }
            }

            if (isCurrentlyHitByFlashlight)
            {
                flashlightHitTimer += Time.deltaTime;
            }
            else
            {
                flashlightHitTimer = 0f;
            }

            if (flashlightHitTimer >= flashlightStunDuration)
            {
                TransitionToCubeForm();
                flashlightHitTimer = 0f;
            }

            isCurrentlyHitByFlashlight = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position,sightRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, walkPointRange);

            Gizmos.color = Color.blue;
            Vector3 rayOrigin = transform.position + Vector3.up * eyeHeightOffset;
            Gizmos.DrawRay(rayOrigin, transform.forward);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, awarenessDistance);
            if (isPH) Gizmos.DrawWireSphere(_lastPlayerPosition, awarenessDistance);

            if (isLPP)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(_lastPlayerPosition, 0.2f);
                Gizmos.DrawLine(transform.position, _lastPlayerPosition);
            }

            if (isMovement)
            {
                Gizmos.color = Color.black;
                var agentPath = agent.path;
                Vector3 prevConrner = transform.position;
                foreach (var c in agentPath.corners) { 
                    Gizmos.DrawLine(prevConrner, c);
                    Gizmos.DrawSphere(c, 0.1f);
                    prevConrner = c;
                }
            }
        }
        #endregion

        private bool IsPlayerInAwarenessRange()
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            return dist <= awarenessDistance;
        }

        private void SearchRotate()
        {
            if (!isSearching) return;

            if (PlayerInSightDuringSearch())
            {
                isSearching = false;
                searchTimer = 0f;
                searchDuration = 0f;
                ChasePlayer();
                return;
            }

            float rotateAmount = currRotationSpeedDuringSearch * Time.deltaTime * rotationDirection;
            currentRotationOffset += rotateAmount;

            // เช็คว่าหมุนเกิน limit แล้วให้กลับทิศ
            if (Mathf.Abs(currentRotationOffset) >= currRotationAngleLimit)
            {
                rotationDirection *= -1; // กลับทิศหมุน
                currentRotationOffset = Mathf.Clamp(currentRotationOffset, -currRotationAngleLimit, currRotationAngleLimit);
            }

            // หมุนตามแนวตั้ง (yaw)
            transform.Rotate(Vector3.up, rotateAmount);

            searchTimer += Time.deltaTime;
            if (searchTimer >= searchDuration)
            {
                isSearching = false;
                searchTimer = 0f;
                searchDuration = 0f;
                isLPP = false;
            }
        }

        private bool PlayerInSightDuringSearch()
        {
            if (!_player) return false;

            Vector3 dirToPlayer = (_player.position - transform.position).normalized;
            float distToPlayer = Vector3.Distance(transform.position, _player.position);
            float viewAngle = Vector3.Angle(transform.forward, dirToPlayer);

            float maxViewAngle = 100f; // สามารถมองได้ในช่วงมุมประมาณ 100 องศา
            float maxViewDistance = sightRange;

            if (viewAngle < maxViewAngle * 0.5f && distToPlayer <= maxViewDistance)
            {
                // ✅ ตรวจว่าไม่มีอะไรมาบัง
                if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, out RaycastHit hit, distToPlayer, _groundLayers))
                {
                    return true;
                }
            }

            return false;
        }

        private void StartIdleLookAround()
        {
            isSearching = true;
            searchTimer = 0f;
            searchDuration = UnityEngine.Random.Range(minSearchDuration,maxSearchDuration);
            currRotationSpeedDuringSearch = rotationSpeedDuringSearch;
            currRotationAngleLimit = rotationAngleLimit;
            
            baseRotation = transform.rotation;
            currentRotationOffset = 0f;
            rotationDirection = 1;

            enemyState.MovementSet(EnemyMovementState.Idling);
            agent.SetDestination(transform.position);
        }

        private void StartActiveSearch()
        {
            isSearching = true;
            searchTimer = 0f;
            searchDuration = UnityEngine.Random.Range(minSearchDuration + 1, maxSearchDuration + 1);
            currRotationSpeedDuringSearch = activeRotationSpeedDuringSearch;
            currRotationAngleLimit = activeRotationAngleLimit;
            
            baseRotation = transform.rotation;
            currentRotationOffset = 0f;
            rotationDirection = 1;

            enemyState.MovementSet(EnemyMovementState.Idling);
            agent.SetDestination(transform.position);
        }

        private void Patroling()
        {
            if (isLPP)
            {
                float d = Vector3.Distance(_player.position,_lastPlayerPosition);
                isPH = true;
                if (d <= awarenessDistance) {
                    _lastPlayerPosition = _player.position;
                    _lastPlayerPosition.y += PlayerController.Instance._characterController.height / 2f;
                }
                _GTPosition = _lastPlayerPosition;
                _positionSet = true;
                isLPP = false;
            }

            if (!_positionSet)
            {
                enemyState.MovementSet(EnemyMovementState.Idling);
                agent.SetDestination(transform.position);
                SearchWalkPoint();
            }
            else {
                enemyState.MovementSet(EnemyMovementState.Walking);
                agent.SetDestination(_GTPosition);
                isMovement = true;
            }

            Vector3 dist = transform.position - _GTPosition;
            if (dist.magnitude < 1f)
            {
                enemyState.MovementSet(EnemyMovementState.Idling);
                _positionSet = false;
                isMovement = false;

                if (isLPP) StartActiveSearch();  // เคยเห็นผู้เล่นมาก่อน → ค้นหาจริงจัง
                else StartIdleLookAround(); // เดินปกติ → มองรอบๆ ชิลๆ

                isPH = false;
                isLPP = false;
            }
        }

        private void SearchWalkPoint()
        {
            float rZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
            float rX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

            Vector3 curr = transform.position;
            _GTPosition = new Vector3(curr.x + rX, curr.y, curr.z + rZ);

            if (Physics.Raycast(_GTPosition, -transform.up, 2f, _groundLayers)) _positionSet = true;
        }

        private void ChasePlayer()
        {
            enemyState.MovementSet(EnemyMovementState.Walking);
            agent.SetDestination(_player.position);
            isLPP = true;
            isMovement = true;
        }

        private void AttackPlayer()
        {
            enemyState.MovementSet(EnemyMovementState.Idling);
            agent.SetDestination(transform.position); // don't move
            isMovement = false;

            if (!isAttack)
            {
                PlayerController.Instance.takeDamage(AttackDamage);

                isAttack = true;
                Invoke(nameof(ResetAtteck), AttackCD);
            }
        }
        private void ResetAtteck()
        {
            isAttack = false;
        }

        private void HitByFlashlight()
        {
            isCurrentlyHitByFlashlight = true;
        }

        private bool CanSeePlayer()
        {
            if (!_player) return false;

            Vector3 directionToPlayer = (_player.position - transform.position).normalized;

            // เช็คว่าผู้เล่นอยู่ในมุมมองหรือไม่
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > viewAngle / 2f) return false;

            // ยิง Raycast ตรวจว่าไม่มีอะไรมาบัง
            Vector3 rayOrigin = transform.position + Vector3.up * eyeHeightOffset;
            Vector3 targetPos = _player.position + Vector3.up * 1.5f; // เผื่อผู้เล่นเตี้ย/สูง
            Vector3 rayDir = (targetPos - rayOrigin).normalized;
            float distance = Vector3.Distance(rayOrigin, targetPos);

            if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, distance, ~0))
            {
                return hit.collider.gameObject == _player.gameObject;
            }

            return false;
        }

        private void TransitionToCubeForm()
        {
            if (enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
                return;
            agent.SetDestination(transform.position);
            astop = true;
            enemyState.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false; // ซ่อน Mesh ของแคปซูล
            if (enemyController != null) enemyController.enabled = false; // ปิด Collider ของแคปซูล

            if (cubeFormPrefab != null)
            {
                // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
                currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation, transform);
                currentCubeFormInstance.tag = gameObject.tag;
                currentCubeFormInstance.layer = gameObject.layer;
                // *** ลบบรรทัดนี้: ไม่ให้ EnemyAI เป็น Parent ของกล่องอีกต่อไป ***
                // currentCubeFormInstance.transform.SetParent(this.transform); 
                // แต่จะให้ MoveableObject เป็นตัวจัดการ Parent ตัวเองกับ Player แทน
            }
        }

        private void TransitionToEnemyForm()
        {
            Vector3 spawnPosition = transform.position;
            float colliderHeight = enemyController != null ? enemyController.height : 0.5f;

            RaycastHit hit;
            Vector3 raycastOrigin = currentCubeFormInstance != null ? currentCubeFormInstance.transform.position + Vector3.up * colliderHeight : transform.position + Vector3.up * colliderHeight;
            if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, colliderHeight, _groundLayers))
            {
                spawnPosition = hit.point + Vector3.up * colliderHeight;
            }
            else
            {
                spawnPosition = currentCubeFormInstance.transform.position + Vector3.up * colliderHeight;
            }

            if (currentCubeFormInstance != null)
            {
                MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
                if (moveableObj != null && moveableObj.IsPicked())
                {
                    moveableObj.Interact();
                }
            }

            if (currentCubeFormInstance != null)
            {
                Destroy(currentCubeFormInstance);
                currentCubeFormInstance = null;
            }

            transform.position = spawnPosition;

            enemyState.ActionSet(EnemyActionState.Idle);
            flashlightHitTimer = 0f;
            cubeFormTimer = 0f;

            astop = false;

            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = true;
            if (enemyController != null) enemyController.enabled = true;
        }

        private void takeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0) die();
        }

        private void die()
        {
            Destroy(gameObject);
        }
    }

}