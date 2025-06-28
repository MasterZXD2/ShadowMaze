using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static Unity.Cinemachine.CinemachineOrbitalTransposer;
using static UnityEditor.SceneView;
using static UnityEngine.UI.Image;

namespace ShadowMaze.EnemyCharactor
{
    public class SimpleEnemy : MonoBehaviour
    {
        #region Class Veriables
        [Header("Enemy Config")]

        [Space(10)]
        [Header("Enemy Components")]

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private EnemyState _enemyState;

        [Space(10)]
        [Header("Enemy State")]

        [SerializeField] private float DetectionRange = 5f;
        [SerializeField] private float Health = 100.0f;
        public float MaxHealth = 100.0f;
        [SerializeField] private float AttackRange = 1.5f;
        [SerializeField] private float AttackDamage = 10.0f;
        [SerializeField] private float AttackCD = 1.0f;

        [Space(10)]
        [Header("Movement")]
        public float EnemyModelRotationSpeed = 10f;

        public float Drag = 0.1f;
        public float MovingThreshold = 0.01f;

        [Header("Walk")]

        public float WalkAcceleration = 0.25f;
        public float WalkSpeed = 4f;

        [Header("Run")]

        public float RunAcceleration = 0.5f;
        public float RunSpeed = 7f;

        [Header("Jump")]

        public float Gravity = 10f;
        public float TerminalVelo = 50f;
        public float JumpSpeed = 1.0f;
        public float InAirAcceleration = 0.15f;
        public float InAirDrag = 0.025f;
        public bool isMovement { get; private set; }
        public bool Sprint { get; private set; }
        public bool Jump { get; private set; }
        public Action hit { get; private set; }

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        // --- ส่วนหัวข้อ "Transformation Settings" (การตั้งค่าการแปลงร่าง) ---
        [Header("Transformation Settings")]
        [SerializeField] private float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง (ตั้งเป็น 0 จะเปลี่ยนทันที)
        [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
        [SerializeField] private MeshRenderer enemyMeshRenderer; // MeshRenderer ของแคปซูล (ศัตรู)
        [SerializeField] private CharacterController enemyController; // Collider ของแคปซูล (ศัตรู)
        [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง

        private Transform _target;
        private Vector2 _rotationDirection = Vector2.zero;

        private string _targetTag;
        private GameObject _playerGameObject;
        private PlayerController _targetController;
        int _targetLayer;

        private bool _jumpedLastFrame = false;
        private float _verticalVelo = 0f;
        private float _anitBump;
        private float _stepOffset;
        private bool isAttack = false;

        private bool astop = false;

        private GameObject currentCubeFormInstance; // ตัวแปรเก็บ Instance ของกล่องที่ถูกสร้างขึ้นมา
        // --- ตัวแปรสถานะ ---
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
            if (!PlayerController.Instance) return;
            _targetController = PlayerController.Instance;
            _playerGameObject = _targetController.gameObject;
        }

        private void Update()
        {
            if (!_targetController) _targetController = PlayerController.Instance;
            if (_targetController && !_playerGameObject) _playerGameObject = _targetController.gameObject;
            MovementState();
            VerticalMovement();
            Vector3 currVec = Vector3.zero;
            currVec.y = _verticalVelo;

            switch (_enemyState.CurrEnemyActionState)
            {
                case EnemyActionState.Idle:
                    HandleIdleState();
                    _characterController.Move(currVec * Time.deltaTime);
                    break;
                case EnemyActionState.Chasing:
                    HandleChasingState();
                    break;
                case EnemyActionState.StunnedAsCube:
                    HandleStunnedAsCubeState();
                    break;
            }

            isCurrentlyHitByFlashlight = false;
        }

        // --- สำหรับแสดง Debug ใน Scene View (Gizmos) ---
        private void OnDrawGizmos()
        {
            // วาดวงกลมสีเหลืองแสดงรัศมีการตรวจจับผู้เล่น (detectionRange)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);

            // วาดวงกลมสีแดงแสดงรัศมีการโจมตีผู้เล่น (attackRange)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);

            Gizmos.color = Color.blue;
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - (_characterController.height / 2f), transform.position.z);
            Gizmos.DrawWireSphere(pos, _characterController.radius);

            Vector3 origin = transform.position - Vector3.up * 0.5f;
            Vector3 dir = transform.forward;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin,dir);
        }

        private void HitByFlashlight()
        {
            //if (_enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube) isCurrentlyHitByFlashlight = true;
            //else if (_enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube && currentCubeFormInstance != null) isCurrentlyHitByFlashlight = true;
            isCurrentlyHitByFlashlight = true;
        }

        private void MovementState()
        {
            _lastMovementState = _enemyState.CurrEnemyMovementState;

            bool isMovinglaterally = IsMovinglaterally(); //matter
            bool isRunning = Sprint && isMovinglaterally; //order matters

            if ((!IsGrounded() || _jumpedLastFrame) && _characterController.velocity.y > 0f)
            {
                _enemyState.MovementSet(EnemyMovementState.Jumping);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else if ((!IsGrounded() || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
            {
                _enemyState.MovementSet(EnemyMovementState.Falling);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else
            {
                EnemyMovementState lateralState = isRunning ? EnemyMovementState.Running :
                    (isMovinglaterally || isMovement ? EnemyMovementState.Walking : EnemyMovementState.Idling);
                _enemyState.MovementSet(lateralState);
                _characterController.stepOffset = _stepOffset;
            }
        }
        private void HandleIdleState()
        {
            // ในสถานะ Idle, จะเริ่มไล่ล่าเมื่อผู้เล่นเข้าใกล้ detectionRange
            if (!_playerGameObject) return;
            float distanceToPlayer = Vector3.Distance(transform.position, _playerGameObject.transform.position);
            if (distanceToPlayer <= DetectionRange) _enemyState.ActionSet(EnemyActionState.Chasing);
        }

        private void HandleChasingState()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerGameObject.transform.position);
            if (distanceToPlayer <= DetectionRange && CanSeePlayer())
            {
                LateralMovement();
                if (distanceToPlayer <= AttackRange)
                {
                    if (isAttack) return;
                    OnAttack();
                }
            }
            else
            {
                StopMove();
                _enemyState.ActionSet(EnemyActionState.Idle);
                flashlightHitTimer = 0f; 
                return;
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
        }

        bool CanSeePlayer()
        {
            if (_playerGameObject == null) return false;
            
            Vector3 dir = (_playerGameObject.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _playerGameObject.transform.position);

            float colliderHeight = enemyController != null ? enemyController.height : 0.5f;

            if (Physics.Raycast(transform.position + Vector3.up * colliderHeight, dir, out RaycastHit hit, distance))
            {
                if (hit.collider.gameObject == _playerGameObject)
                    return true; // มองเห็น
            }
            return false; // มีบางอย่างบัง
        }

        private void VerticalMovement()
        {
            if (astop || _enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube) return;

            bool isGrounded = _enemyState.InGroundedState();
 
            _verticalVelo -= Gravity * Time.deltaTime;

            if (isGrounded && _verticalVelo < 0) _verticalVelo = -_anitBump;

            if (ObstacleInFront() && isGrounded)
            {
                _verticalVelo += _anitBump * Mathf.Sqrt(JumpSpeed * Gravity);
                _jumpedLastFrame = true;
            }

            if (_enemyState.InStateGroundedState(_lastMovementState) && !isGrounded) _verticalVelo += _anitBump;

            if (Mathf.Abs(_verticalVelo) > Mathf.Abs(TerminalVelo)) _verticalVelo = -1f * Mathf.Abs(TerminalVelo);
        }

        void LateralMovement()
        {
            if (astop) return;
            
            bool isGrounded = _enemyState.InGroundedState();

            isMovement = true;
            Vector3 heading = (_playerGameObject.transform.position - transform.position);
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
            newVelo.y += _verticalVelo;
            newVelo = !isGrounded ? SteepWalls(newVelo) : newVelo;
            _characterController.Move(newVelo * Time.deltaTime);
        }
        bool ObstacleInFront()
        {
            Vector3 origin = transform.position - Vector3.up * 0.5f;
            Vector3 dir = transform.forward;
            return Physics.Raycast(origin, dir, 1.0f, _groundLayers); // ระยะ 1 เมตรข้างหน้า
        }

        private void StopMove()
        {
            isMovement = false;

            Vector3 currentVelo = _characterController.velocity;
            Vector3 stopVelo = new Vector3(0f, currentVelo.y, 0f);
            _characterController.Move(stopVelo * Time.deltaTime);
        }

        private Vector3 SteepWalls(Vector3 velo)
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _characterController.slopeLimit;

            if (!validAngle && _verticalVelo < 0f)
            {
                velo = Vector3.ProjectOnPlane(velo, normal);
            }

            return velo;
        }

        private void RotatePlayerToTar(Vector3 dir)
        {
            if (_playerGameObject == null) return;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            //targetRotation *= Quaternion.Euler(0f, 90f, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, EnemyModelRotationSpeed * Time.deltaTime);
        }

        private bool IsMovinglaterally()
        {
            Vector3 lateralVelo = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
            return lateralVelo.magnitude > MovingThreshold;
        }

        private bool IsGrounded()
        {
            bool grounded = _enemyState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - (_characterController.height / 2f), transform.position.z);
            bool grounded = Physics.CheckSphere(pos, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _characterController.slopeLimit;

            return _characterController.isGrounded && validAngle;
        }


        private void HandleStunnedAsCubeState()
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
        }

        private void TransitionToCubeForm()
        {
            if (_enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube)
                return;
            astop = true;
            _enemyState.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false; // ซ่อน Mesh ของแคปซูล
            if (enemyController != null) enemyController.enabled = false; // ปิด Collider ของแคปซูล

            if (cubeFormPrefab != null)
            {
                // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
                currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation,transform);
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

            _enemyState.ActionSet(EnemyActionState.Idle);
            flashlightHitTimer = 0f;
            cubeFormTimer = 0f;

            astop = false;

            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = true;
            if (enemyController != null) enemyController.enabled = true;
        }

        private void OnAttack()
        {
            _targetController.takeDamage(AttackDamage);
            isAttack = true;
            Invoke(nameof(CanAttack), AttackCD);
        }
        private void CanAttack() => isAttack = false;

        public void takeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0) die();
        }

        private void die()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}