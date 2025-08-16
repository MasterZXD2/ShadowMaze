using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

namespace ShadowMaze.EnemyCharactor
{
    public class SimpleEnemyAi : WhatToFear
    {
        #region Class Veriables
        [Header("Enemy Config")]

        [Space(10)]
        [Header("Enemy Components")]
        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected EnemyState enemyState;
        [SerializeField] protected GameObject enemyModel;

        [Space(10)]
        [Header("Stats")]
        public float Health = 100f;
        public float MaxHealth = 100f;

        [Space(10)]
        [Header("Attack")]
        [SerializeField] protected float attackRange = 1.8f;
        [SerializeField] private float AttackCD = .5f;
        [SerializeField] private float AttackDamage = 10f;

        [Space(10)]
        [Header("Move")]
        [SerializeField] private float walkPointRange = 12f;
        [SerializeField] private float EnemyModelRotationSpeed = 10f;

        private float WalkAcceleration = 0.25f;
        private float WalkSpeed = 4f;

        [Header("Jump")]
        public float Gravity = 10f;
        public float TerminalVelo = 10f;
        public float JumpSpeed = 8f;
        public float InAirAcceleration = 40f;
        public float InAirDrag = 1f;

        [Header("Sight Settings")]
        [SerializeField] protected float sightRange = 20f;
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private float eyeHeightOffset = .5f;

        [Space(10)]
        [Header("Search Settings")]
        [SerializeField] private float minSearchDuration = 1f;
        [SerializeField] private float maxSearchDuration = 3f;
        [SerializeField] private float rotationAngleLimit = 120f;
        [SerializeField] private float activeRotationAngleLimit = 90f;
        [SerializeField] private float rotationSpeedDuringSearch = 100f;
        [SerializeField] private float activeRotationSpeedDuringSearch = 200f;

        [Space(10)]
        [Header("Awareness")]
        [SerializeField] private float awarenessDistance = 4f; // ระยะที่ศัตรูจะรู้ว่าผู้เล่นอยู่ใกล้ แม้จะมองไม่เห็น

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] protected LayerMask _groundLayers;

        [Space(10)]
        [Header("Transformation Settings")]
        [SerializeField] protected float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง (ตั้งเป็น 0 จะเปลี่ยนทันที)
        [SerializeField] protected CharacterController enemyController; // Collider ของแคปซูล (ศัตรู)

        protected bool isMovement { get; set; }
        public bool isJump { get; private set; }
        public bool isAttacking { get; private set; }
        protected bool isCanSeePlayer { get; set; }

        protected Vector3 spawnPoint;

        private bool _jumpedLastFrame = false;
        private float _verticalVelo = 0f;
        private float _anitBump;
        private float _stepOffset;

        private bool isSearching = false;
        private float searchTimer = 0f;
        private float searchDuration = 0f;
        private float currRotationAngleLimit = 60f;
        private float currRotationSpeedDuringSearch = 60f;
        private Quaternion baseRotation;
        private float currentRotationOffset = 0f; // หมุนสะสม
        private int rotationDirection = 1; // 1 = ขวา, -1 = ซ้าย

        private bool isPlayerInSight, isPlayerInAttack;
        private Vector3 _GTPosition;
        protected bool _positionSet;
        protected bool isAttack;
        protected Vector3 _lastPlayerPosition;
        protected float _sameGTPositionTimer = 0f;
        protected Vector3 _lastGTPosition;
        protected bool isLPP = false;
        private bool isPH = false;
        protected Vector3 _offset = Vector3.zero;

        protected bool InMemeoryFragmentArea = false;
        protected bool astop = false;
        protected bool stun = false;

        protected bool isCurrentlyHitByFlashlight = false; // สถานะปัจจุบันว่ากำลังโดนไฟฉายส่องหรือไม่
        protected float flashlightHitTimer = 0f; // นับเวลาที่โดนไฟฉายส่อง (เพื่อเปลี่ยนเป็นกล่อง)

        protected EnemyMovementState _lastMovementState = EnemyMovementState.Falling;
        #endregion

        #region Normal Function
        protected virtual void Awake() {
            EnemyManager.Instance.Add(gameObject);

            MaxHealth = Health;
            isCanSeePlayer = true;
            enemyController.enabled = false;
            if (!agent) agent = GetComponent<NavMeshAgent>();
            agent.enabled = true;
        }

        protected override void Start()
        {
            if (enemyModel) _offset = enemyModel.transform.position - transform.position;
            spawnPoint = transform.position;

            FindPlayer();
            if (!agent) agent = GetComponent<NavMeshAgent>();

            WalkSpeed = agent.speed * 1.1f;
            WalkAcceleration = agent.acceleration * 5;
        }

        protected override void Update()
        {
            if (!_player || isJump) return;
            PlayerCheck();
            if (enemyModel)
            {
                enemyModel.transform.position = transform.position + _offset;
                enemyModel.transform.rotation = transform.rotation;
            }
            if (astop)
            {
                agent.SetDestination(transform.position);
                return;
            }
            OnFlashLight();

            if (!stun)
            {
                float PH = PlayerController.Instance._characterController.height / 2f;
                Vector3 _tarVec = _player.position;
                _tarVec.y += PH;

                if (ShouldJumpTo(_tarVec) && agent.pathStatus != NavMeshPathStatus.PathComplete && !astop)
                {
                    _sameGTPositionTimer = 0f;
                    StartCoroutine(JumpTo(_player.position));
                    return;
                }
                else if (_sameGTPositionTimer > 1.2f && !isAttacking && isMovement && !astop)
                {
                    Vector3 point;
                    if (GetGroundAtAngle(transform.position + Vector3.up * eyeHeightOffset, out point))
                    {
                        if (ShouldJumpTo(point))
                        {
                            _sameGTPositionTimer = 0f;
                            StartCoroutine(JumpTo(point));
                            return;
                        }
                    }
                    else
                    {
                        if (!isCanSeePlayer) return;
                        _sameGTPositionTimer = 0f;
                        _positionSet = false;
                        isCanSeePlayer = false;
                        Invoke(nameof(MakeCanSeePlayer), 2f);
                        return;
                    }
                }

                if (isSearching && !astop)
                {
                    SearchRotate();
                    return;
                }

                Move(PH);
            }

            isCurrentlyHitByFlashlight = false;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

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

        #region Movement Function
        protected virtual void Move(float PH) => NormalMove(PH);

        protected virtual void NormalMove(float PH)
        {
            if (astop) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= sightRange && isCanSeePlayer)
            {
                if (CanSeePlayer())
                {
                    _lastPlayerPosition = _player.position;
                    _lastPlayerPosition.y += PH;

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
                    _lastPlayerPosition.y += PH;
                    ChasePlayer();
                }
                else
                {
                    Patroling();
                    //flashlightHitTimer = 0f;
                    //return;
                }
            }
            else
            {
                Patroling();
                //flashlightHitTimer = 0f;
                //return;
            }

            if (!isAttacking)
            {
                if (transform.position == _lastGTPosition) _sameGTPositionTimer += Time.deltaTime;
                else _sameGTPositionTimer = 0f;
            }

            _lastGTPosition = transform.position;
        }

        protected virtual void OnFlashLight()
        {
            if (isCurrentlyHitByFlashlight)
            {
                flashlightHitTimer += Time.deltaTime;
            }
            else
            {
                flashlightHitTimer = 0f;
            }

            if (flashlightHitTimer >= flashlightStunDuration) die();
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
            isAttacking = false;
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

        protected void ChasePlayer()
        {
            enemyState.MovementSet(EnemyMovementState.Walking);
            agent.SetDestination(_lastPlayerPosition);
            isLPP = true;
            isAttacking = false;
            isMovement = true;
        }

        protected void AttackPlayer()
        {
            //enemyState.MovementSet(EnemyMovementState.Idling);
            //agent.SetDestination(transform.position); // don't move
            //isMovement = false;
            //isAttacking = true;

            if (!isAttack)
            {
                //PlayerController.Instance._playerStats.Sanity -= AttackDamage;
                PlayerController.Instance.takeSanity(AttackDamage);
                isAttack = true;
                Invoke(nameof(ResetAtteck), AttackCD);
            }
            AAP();
        }

        protected virtual void AAP() => die();

        IEnumerator JumpTo(Vector3 targetPos)
        {
            // ปิดการเคลื่อนที่ปกติ
            isJump = true;
            isMovement = false;

            agent.enabled = false;
            enemyController.enabled = true;

            float v0y = JumpSpeed;
            float v0x = WalkSpeed;
            float g = Gravity;

            Vector3 horizontalDir = (new Vector3(targetPos.x, 0, targetPos.z)
                                     - new Vector3(transform.position.x, 0, transform.position.z))
                                    .normalized;

            float dx = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPos.x, 0, targetPos.z)
            );
            float totalTime = dx / v0x;
            float elapsed = 0f;

            // 2) กระโดดตามพาราโบลา
            while (elapsed < totalTime)
            {
                // คำนวณ velocity ณ เวลานั้น
                float vy = v0y - g * elapsed;
                Vector3 vel = horizontalDir * v0x + Vector3.up * vy;
                enemyController.Move(vel * Time.deltaTime);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 3) รอให้ลงถึงพื้น (เช็ก CharacterController.isGrounded)
            while (!enemyController.isGrounded)
            {
                enemyController.Move(Vector3.down * g * Time.deltaTime);
                yield return null;
            }

            // ลงจอดแล้วกลับไปเดินปกติ
            isMovement = true;

            float followTime = 0f;
            float maxFollow = 0.5f;
            while (followTime < maxFollow)
            {
                Vector3 dir = (_player.position - transform.position);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Vector3 step = dir.normalized * v0x * Time.deltaTime;
                    enemyController.Move(step);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(dir),
                        EnemyModelRotationSpeed * Time.deltaTime
                    );
                }
                followTime += Time.deltaTime;
                yield return null;
            }

            agent.enabled = true;
            enemyController.enabled = false;

            _positionSet = false;

            isMovement = false;
            isJump = false;

            agent.Warp(transform.position);
            agent.SetDestination(_player.position);
        }

        /*
        private void ResetAtteck()
        {
            isAttack = false;
        }
        */
        #endregion

        #region Check Function
        protected bool IsPlayerInAwarenessRange()
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            return dist <= awarenessDistance;
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

        public bool GetGroundAtAngle(Vector3 origin, out Vector3 hitTopPoint)
        {
            hitTopPoint = Vector3.zero;

            // ยิงลงแบบทำมุม 45 องศา (ด้านหน้า + ลง)
            Vector3 forward = transform.forward;
            Vector3 down = Vector3.down;

            // สร้างทิศทาง 45 องศา: ปกติจะ normalize ก่อน lerp
            Vector3 direction = (forward + down).normalized;

            // ระยะยิง
            float rayDistance = 5f;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, _groundLayers))
            {
                // hit.point คือจุดชน
                // hit.normal คือ normal ของพื้น

                // ปรับตำแหน่งให้ขึ้นบนสุดของพื้น (เช่นเอาไว้ยืน)
                hitTopPoint = hit.point + hit.normal * 0.1f; // ดันออกเล็กน้อยเพื่อไม่จม
                return true;
            }

            return false;
        }

        bool ShouldJumpTo(Vector3 targetPos)
        {
            Vector3 start = transform.position;
            Vector3 end = targetPos;
            float dx = Vector3.Distance(
                new Vector3(start.x, 0, start.z),
                new Vector3(end.x, 0, end.z)
            );
            float dy = end.y - start.y;
            float v0y = JumpSpeed;     // ความเร็วเริ่มต้นขึ้น
            float v_max = WalkSpeed;
            float a = InAirAcceleration;
            float g = Gravity;       // ค่า gravity ในเกม

            // 1) ตรวจความสูงสูงสุด
            float Hmax = (v0y * v0y) / (2f * g);
            if (Hmax + 0.1f < dy) return false; // สูงเกินปืนกระโดด

            // 2) เวลาทั้งหมดของกระโดด
            float T = 2f * v0y / g;

            // 3) คำนวณระยะสูงสุดแนวราบเมื่อมี acceleration
            float tAcc = v_max / a;
            float Dmax;
            if (T <= tAcc)
            {
                // กระโดดทั้งกระบอกยังไม่ถึง v_max
                Dmax = 0.5f * a * T * T;
            }
            else
            {
                // เร่งถึง v_max แล้วเดินด้วย v_max ถึงจบ
                float D1 = 0.5f * a * tAcc * tAcc;
                float D2 = v_max * (T - tAcc);
                Dmax = D1 + D2;
            }
            //Debug.Log(dx.ToString() + "," + Dmax.ToString());
            if (dx > Dmax + 0.1f) return false;

            // 4) ตรวจ trajectory ณ ระยะ dx
            //   t_dx: เวลา ณ ตำแหน่งแนวราบ dx
            float t_dx;
            if (dx <= 0.5f * a * tAcc * tAcc)
            {
                // ยังอยู่ในช่วงเร่ง
                // x = 0.5 a t^2  → t = sqrt(2 x / a)
                t_dx = Mathf.Sqrt(2f * dx / a);
            }
            else
            {
                // แบ่งระยะ: D1 + v_max*(t_dx - tAcc) = dx
                float D1 = 0.5f * a * tAcc * tAcc;
                t_dx = tAcc + (dx - D1) / v_max;
            }
            // ความสูง y ณ t_dx
            float yAtDx = v0y * t_dx - 0.5f * g * t_dx * t_dx;
            if (yAtDx + 0.1f < dy) return false;

            return true;
        }

        protected bool CanSeePlayer()
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
        #endregion

        #region Control Functions

        /*
        private void takeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0) die();
        }
        */
        private void ResetAtteck() => isAttack = false;

        private void MakeCanSeePlayer() => isCanSeePlayer = true;

        public void die()
        {
            EnemyManager.Instance.Remove(gameObject);
            Destroy(gameObject);
        }

        public void pause() => astop = true;
        public void resume() => astop = false;

        /*
        private void disappear()
        {
            agent.SetDestination(transform.position);

            _positionSet = false;
            isMovement = false;
            isAttack = false;
            isLPP = false;
            isCanSeePlayer = false;

            astop = true;

            if (enemyMeshRenderer) enemyMeshRenderer.enabled = false;
            if (enemyController) enemyController.enabled = false;
            //if (agent) agent.enabled = false;
        }
        private void DelayedRespawn() => respawn(spawnPoint);
        

        private void respawn(Vector3 point)
        {
            transform.position = spawnPoint;
            agent.Warp(transform.position);
            agent.SetDestination(transform.position);

            enemyState.ActionSet(EnemyActionState.Idle);
            flashlightHitTimer = 0f;
            cubeFormTimer = 0f;

            isCanSeePlayer = true;
            astop = false;

            if (enemyMeshRenderer) enemyMeshRenderer.enabled = true;
            if (enemyController) enemyController.enabled = true;
            if (agent) agent.enabled = true;
        }
        */

        public virtual void HitByFlashlight(bool b = false) => isCurrentlyHitByFlashlight = true;
        
        public void InArea(string name)
        {
            if (name == "MFA") InMemeoryFragmentArea = true;
        }
        
        public void OutArea()
        {
            InMemeoryFragmentArea = false;
        }
        #endregion
    }

}