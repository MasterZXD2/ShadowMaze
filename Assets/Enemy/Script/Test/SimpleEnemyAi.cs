using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using UnityEngine;
using UnityEngine.AI;

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
        [SerializeField] private float sightRange;
        [SerializeField] private float walkPointRange;
        [SerializeField] private float EnemyModelRotationSpeed = 10f;

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers, _playerLayer;

        [Header("Transformation Settings")]
        [SerializeField] private float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง (ตั้งเป็น 0 จะเปลี่ยนทันที)
        [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
        [SerializeField] private MeshRenderer enemyMeshRenderer; // MeshRenderer ของแคปซูล (ศัตรู)
        [SerializeField] private CharacterController enemyController; // Collider ของแคปซูล (ศัตรู)
        [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง

        public Action hit { get; private set; }

        private bool isPlayerInSight, isPlayerInAttack;
        private Transform _player;
        private Vector3 _GTPosition;
        private bool _positionSet;
        private bool isAttack;

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
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist <= sightRange && CanSeePlayer())
                {
                    if (astop) return;
                    RotatePlayerToTar(_player.position);
                    if (dist <= attackRange)
                    {
                        AttackPlayer();
                    }
                    else
                    {
                        ChasePlayer();
                    }
                }
                else
                {
                    if (astop) return;
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
        }
        #endregion

        private void Patroling()
        {
            if (!_positionSet)
            {
                enemyState.MovementSet(EnemyMovementState.Idling);
                SearchWalkPoint();
            }
            else {
                enemyState.MovementSet(EnemyMovementState.Walking);
                agent.SetDestination(_GTPosition); 
            }

            Vector3 dist = transform.position - _GTPosition;
            if (dist.magnitude < 1f) 
            {
                enemyState.MovementSet(EnemyMovementState.Idling);
                _positionSet = false;
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
        }

        private void RotatePlayerToTar(Vector3 dir)
        {
            if (!_player) return;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            //targetRotation *= Quaternion.Euler(0f, 90f, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, EnemyModelRotationSpeed * Time.deltaTime);
        }

        private void AttackPlayer()
        {
            enemyState.MovementSet(EnemyMovementState.Idling);
            agent.SetDestination(transform.position); // don't move

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

        bool CanSeePlayer()
        {
            if (_player == null) return false;

            Vector3 origin = transform.position + Vector3.up * 1.0f; // จุดเริ่มต้นของ ray (ระดับหัวศัตรู)
            Vector3 targetPos = _player.position + Vector3.up * 1.0f; // จุดเป้าหมาย (ระดับหัวผู้เล่น)
            Vector3 direction = (targetPos - origin).normalized;
            float distance = Vector3.Distance(origin, targetPos);

            // ยิง Ray เพื่อเช็กสิ่งที่อยู่ตรงกลางระหว่างศัตรูกับผู้เล่น
            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _groundLayers))
            {
                // ถ้ามีอะไรขวางอยู่ และไม่ใช่ผู้เล่น => ศัตรูมองไม่เห็น
                if (hit.collider.gameObject != _player.gameObject)
                {
                    Debug.DrawLine(origin, hit.point, Color.red, 0.2f); // Debug เส้นสีแดง (ไม่เห็น)
                    return false;
                }
            }

            Debug.DrawLine(origin, targetPos, Color.green, 0.2f); // Debug เส้นสีเขียว (เห็น)
            return true;
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