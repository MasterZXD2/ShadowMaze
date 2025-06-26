using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.SceneView;

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
        [SerializeField] private Collider enemyCollider; // Collider ของแคปซูล (ศัตรู)
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
            _targetController = PlayerController.Instance;
            _playerGameObject = _targetController.gameObject;
            hit = HitByFlashlight;
        }

        private void Update()
        {
            if (!_targetController) _targetController = PlayerController.Instance;
            if (!_playerGameObject) _playerGameObject = _targetController.gameObject;
            isCurrentlyHitByFlashlight = false;
            MovementState();
        }

        private void HitByFlashlight()
        {
            if (_enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube) isCurrentlyHitByFlashlight = true;
            else if (_enemyState.CurrEnemyActionState == EnemyActionState.StunnedAsCube && currentCubeFormInstance != null) isCurrentlyHitByFlashlight = true;
        }

        private void MovementState()
        {
            switch (_enemyState.CurrEnemyActionState)
            {
                case EnemyActionState.Idle:
                    HandleIdleState();
                    break;
                case EnemyActionState.Chasing:
                    HandleChasingState();
                    break;
                case EnemyActionState.StunnedAsCube:
                    HandleStunnedAsCubeState();
                    break;
            }
        }
        private void HandleIdleState()
        {
            // ในสถานะ Idle, จะเริ่มไล่ล่าเมื่อผู้เล่นเข้าใกล้ detectionRange
            float distanceToPlayer = Vector3.Distance(transform.position, _playerGameObject .transform.position);
            if (distanceToPlayer <= DetectionRange) _enemyState.ActionSet(EnemyActionState.Chasing);
        }

        private void HandleChasingState()
        {
            // Logic การไล่ตาม Player
            float distanceToPlayer = Vector3.Distance(transform.position, _playerGameObject .transform.position);
            if (distanceToPlayer <= DetectionRange)
            {
                /*
                Vector3 directionToPlayer = (_playerGameObject .transform.position - transform.position).normalized;
                // ใช้ Rigidbody.MovePosition ถ้ามี NavMeshAgent ปิดอยู่
                if (navMeshAgent != null && !navMeshAgent.enabled && rb != null && !rb.isKinematic)
                {
                    rb.MovePosition(transform.position + directionToPlayer * moveSpeed * Time.deltaTime);
                }
                else // ถ้าไม่มี Rigidbody หรือ Rigidbody เป็น kinematic หรือ NavMeshAgent ทำงานอยู่
                {
                    transform.position += directionToPlayer * moveSpeed * Time.deltaTime; // Fallback หรือใช้ NavMeshAgent แทน
                }

                Vector3 lookAtPlayer = _playerGameObject .transform.position;
                lookAtPlayer.y = transform.position.y; // ทำให้ศัตรูหันเฉพาะแกน Y
                transform.LookAt(lookAtPlayer); // หันหน้าเข้าหาผู้เล่น

                // โจมตีผู้เล่นถ้าอยู่ในระยะ attackRange และ Cooldown หมดแล้ว
                if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
                {
                    AttackPlayer();
                }
                */
            }
            else
            {
                // ถ้าผู้เล่นออกนอกระยะ detectionRange ให้กลับสู่สถานะ Idle
                _enemyState.ActionSet(EnemyActionState.Idle);
                flashlightHitTimer = 0f; // รีเซ็ต timer เมื่อออกจากการไล่ตาม
                return; // ออกจากเมธอดเมื่อเปลี่ยน State
            }

            // อัปเดต flashlightHitTimer (นับเวลาที่โดนไฟฉายส่อง)
            if (isCurrentlyHitByFlashlight)
            {
                flashlightHitTimer += Time.deltaTime;
            }
            else
            {
                flashlightHitTimer = 0f; // รีเซ็ต timer หากไฟฉายหยุดส่อง
            }

            // ตรวจสอบว่าถึงเวลาเปลี่ยนเป็นกล่องแล้วหรือยัง
            if (flashlightHitTimer >= flashlightStunDuration)
            {
                TransitionToCubeForm(); // เปลี่ยนเป็นร่างกล่อง
                flashlightHitTimer = 0f; // รีเซ็ต timer หลังจากเปลี่ยนสถานะ
            }
        }

        private void HandleStunnedAsCubeState()
        {
            // ถ้ายังโดนไฟฉายส่องอยู่ (โดนกล่อง) ให้คงเป็นกล่องต่อไป และรีเซ็ต timer กลับร่างเดิม
            if (isCurrentlyHitByFlashlight)
            {
                cubeFormTimer = 0f; // รีเซ็ต timer การนับถอยหลังกลับร่าง
            }
            else // ถ้าไม่โดนไฟฉายส่องแล้ว (กล่องไม่โดนแสง)
            {
                cubeFormTimer += Time.deltaTime; // เริ่มนับถอยหลังเพื่อกลับร่างเดิม
                if (cubeFormTimer >= cubeFormDuration)
                {
                    TransitionToEnemyForm(); // กลับคืนร่างศัตรู
                    cubeFormTimer = 0f; // รีเซ็ต timer หลังจากกลับร่าง
                }
            }

            // *** ลบโค้ดนี้ออก: EnemyAI ไม่ควรเป็น Parent ของกล่องอีกต่อไป แต่จะตามตำแหน่งใน FixedUpdate แทน ***
            // if (currentCubeFormInstance != null)
            // {
            //     MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
            //     if (moveableObj != null && moveableObj.IsPicked())
            //     {
            //         transform.position = currentCubeFormInstance.transform.position;
            //         transform.rotation = currentCubeFormInstance.transform.rotation;
            //     }
            // }
        }

        private void TransitionToCubeForm()
        {
            _enemyState.ActionSet(EnemyActionState.StunnedAsCube); // เปลี่ยนสถานะเป็น StunnedAsCube

            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false; // ซ่อน Mesh ของแคปซูล
            if (enemyCollider != null) enemyCollider.enabled = false; // ปิด Collider ของแคปซูล

            // ทำให้ Rigidbody ของ EnemyAI ตัวนี้เป็น Kinematic และหยุด NavMeshAgent
            // เพื่อไม่ให้มันได้รับผลกระทบจากฟิสิกส์ หรือเคลื่อนที่เองขณะเป็นกล่อง
            /*
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero; // หยุดการเคลื่อนที่ที่เหลืออยู่
                rb.angularVelocity = Vector3.zero; // หยุดการหมุนที่เหลืออยู่
            }
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true; // สั่งหยุด NavMeshAgent
                navMeshAgent.enabled = false; // ปิด NavMeshAgent ชั่วคราว
            }
            */
            if (cubeFormPrefab != null)
            {
                // สร้างกล่องที่ตำแหน่งและ Rotation ปัจจุบันของ EnemyAI (แคปซูล)
                currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation);

                // *** ลบบรรทัดนี้: ไม่ให้ EnemyAI เป็น Parent ของกล่องอีกต่อไป ***
                // currentCubeFormInstance.transform.SetParent(this.transform); 
                // แต่จะให้ MoveableObject เป็นตัวจัดการ Parent ตัวเองกับ Player แทน
            }
        }

        private void TransitionToEnemyForm()
        {
            // ก่อนจะกลับร่าง ให้ Unparent กล่องออกจาก Player (ถ้ากล่องถูกหยิบอยู่)
            // หรือ Unparent จาก EnemyAI ถ้ายัง Parent อยู่ (ซึ่งไม่ควรเกิดขึ้นในเวอร์ชันนี้)
            if (currentCubeFormInstance != null)
            {
                MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
                if (moveableObj != null && moveableObj.IsPicked())
                {
                    // ถ้ากล่องถูก Player หยิบอยู่, ให้ MoveableObject จัดการ Unparent ตัวเอง
                    // เราแค่เรียก Interact() อีกครั้งเพื่อสั่งให้มันปล่อย
                    moveableObj.Interact();
                }
                // else if (currentCubeFormInstance.transform.parent == this.transform) // กรณีหลงเหลือที่ EnemyAI เป็น parent
                // {
                //     currentCubeFormInstance.transform.parent = null; 
                // }
            }

            // ทำลาย instance ของกล่อง (ไม่ใช่ prefab)
            if (currentCubeFormInstance != null)
            {
                Destroy(currentCubeFormInstance);
                currentCubeFormInstance = null;
            }

            // คืนค่า Rigidbody และ NavMeshAgent ของ EnemyAI ตัวนี้กลับเป็นปกติ
            /*
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true; // เปิด NavMeshAgent กลับมา
                navMeshAgent.isStopped = false; // สั่งให้มันเริ่มเคลื่อนที่ได้
            }
            */
            // คำนวณตำแหน่งที่ปลอดภัยบนพื้นสำหรับศัตรู (แคปซูล)
            // ตำแหน่งปัจจุบันของ EnemyAI คือตำแหน่งสุดท้ายของแคปซูลก่อนเปลี่ยนร่าง
            // หรือถ้ากล่องถูกหยิบ มันจะอยู่ที่ตำแหน่งสุดท้ายที่กล่องเคยอยู่
            Vector3 spawnPosition = transform.position;
            // ใช้ Bounds.extents.y สำหรับครึ่งหนึ่งของความสูง collider เพื่อให้ฐานของ collider อยู่บนพื้นพอดี
            float colliderHeight = enemyCollider != null ? enemyCollider.bounds.extents.y : 0.5f;

            RaycastHit hit;
            // ยิง Raycast จากด้านบนลงมา (สูง 5 หน่วยจากตำแหน่งกล่อง) เพื่อหาพื้น (ในระยะ 15 หน่วย)
            // หาก currentCubeFormInstance ไม่ใช่ null และกล่องไม่ได้ถูกหยิบไปไหน
            // ให้ใช้ตำแหน่งของกล่องเป็นจุดเริ่มต้น Raycast เพื่อความแม่นยำ
            Vector3 raycastOrigin = currentCubeFormInstance != null ? currentCubeFormInstance.transform.position + Vector3.up * 5f : transform.position + Vector3.up * 5f;

            if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, 15f, _groundLayers))
            {
                spawnPosition = hit.point + Vector3.up * colliderHeight; // ปรับตำแหน่งให้อยู่บนพื้นพอดี
                Debug.Log($"EnemyAI: Found ground at {hit.point}, spawning at {spawnPosition}");
            }
            else
            {
                Debug.LogError($"Enemy '{gameObject.name}' could NOT find ground to respawn on! Spawning at last known position ({spawnPosition}). This might lead to falling through the map. Check groundLayer and environment.");
            }

            // ตั้งค่าตำแหน่งของ EnemyAI GameObject (แคป) ไปยังตำแหน่งที่คำนวณได้
            transform.position = spawnPosition;
            // อาจจะรีเซ็ต Rotation ด้วย ถ้าต้องการ
            // transform.rotation = Quaternion.identity; 

            // กลับสู่สถานะ Idle และรีเซ็ต Timer ต่างๆ
            _enemyState.ActionSet(EnemyActionState.Idle);
            flashlightHitTimer = 0f;
            cubeFormTimer = 0f;

            // แสดง Mesh และเปิด Collider ของแคปซูลกลับมา
            if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = true;
            if (enemyCollider != null) enemyCollider.enabled = true;
        }

        // --- สำหรับแสดง Debug ใน Scene View (Gizmos) ---
        private void OnDrawGizmosSelected()
        {
            // วาดวงกลมสีเหลืองแสดงรัศมีการตรวจจับผู้เล่น (detectionRange)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);

            // วาดวงกลมสีแดงแสดงรัศมีการโจมตีผู้เล่น (attackRange)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
        #endregion
        /*
        private void Update()
        {
            if (astop) return;

            MovementState();
            VerticalMovement();

            if (_target)
            {
                LateralMovement();
                if (!isAttack)
                {
                    float mag = (_target.position - transform.position).magnitude;
                    if (mag < 3) Attack();
                }
            }
            else StopMove();
        }

        private void LateUpdate()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.azure;
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - _characterController.height, transform.position.z);
            Gizmos.DrawWireSphere(pos, _characterController.radius);
        }

        #endregion

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

        private void StopMove()
        {
            isMovement = false;

            Vector3 currentVelo = _characterController.velocity;
            Vector3 stopVelo = new Vector3(0f, currentVelo.y, 0f);
            _characterController.Move(stopVelo * Time.deltaTime);
        }

        private void VerticalMovement()
        {
            bool isGrounded = _enemyState.InGroundedState();

            _verticalVelo -= Gravity * Time.deltaTime;

            if (isGrounded && _verticalVelo < 0) _verticalVelo = -_anitBump;

            if (Jump && isGrounded)
            {
                _verticalVelo += _anitBump * Mathf.Sqrt(JumpSpeed * Gravity);
                _jumpedLastFrame = true;
            }

            if (_enemyState.InStateGroundedState(_lastMovementState) && !isGrounded) _verticalVelo += _anitBump;

            if (Mathf.Abs(_verticalVelo) > Mathf.Abs(TerminalVelo)) _verticalVelo = -1f * Mathf.Abs(TerminalVelo);
        }

        void LateralMovement()
        {
            bool isGrounded = _enemyState.InGroundedState();

            isMovement = true;
            Vector3 heading = (_target.position - transform.position);
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
            if (_target == null) return;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            targetRotation *= Quaternion.Euler(0f, 90f, 0f);
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
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - _characterController.height, transform.position.z);
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

        private void Attack()
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
            PlayerStats PS = _target.GetComponent<PlayerStats>();
            if (!PS.isDie) PS.UP(exp);
            Destroy(gameObject);
        }

        public void pause() => astop = true;
        public void resume() => astop = false;
        */
    }
}