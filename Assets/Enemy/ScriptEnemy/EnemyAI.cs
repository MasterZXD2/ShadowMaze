using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShadowMaze.MyCharacterController;
using ShadowMaze.InteractSystem;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // --- [Header("Enemy Stats")] ---
    [Header("Enemy Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime;

    // --- [Header("Transformation Settings")] ---
    [Header("Transformation Settings")]
    [SerializeField] private float flashlightStunDuration = 3f;
    [SerializeField] private float cubeFormDuration = 5f;
    [SerializeField] private MeshRenderer enemyMeshRenderer;
    [SerializeField] private Collider enemyCollider;
    [SerializeField] private GameObject cubeFormPrefab;
    [SerializeField] private LayerMask groundLayer; // สำหรับ Raycast หาพื้น

    // --- Private References (อ้างอิงถึง Component อื่นๆ) ---
    private GameObject playerGameObject;
    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerLight playerLight;
    private Rigidbody rb; // Rigidbody ของตัว EnemyAI เอง
    private NavMeshAgent navMeshAgent; // NavMeshAgent ของตัว EnemyAI เอง (ถ้ามี)

    private GameObject currentCubeFormInstance; // Instance ของกล่องที่ถูกสร้างขึ้น

    // --- State Variables ---
    private bool isCurrentlyHitByFlashlight = false;
    private float flashlightHitTimer = 0f;
    private float cubeFormTimer = 0f;

    // --- Enemy State Enum ---
    private enum EnemyState
    {
        Idle,
        Chasing,
        StunnedAsCube
    }
    private EnemyState currentState = EnemyState.Idle; // State ปัจจุบันของศัตรู

    // --- Unity Lifecycle Methods ---
    private void Start()
        {
            // ค้นหา Player GameObject และ Components ที่เกี่ยวข้อง
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject != null)
            {
                playerController = playerGameObject.GetComponent<PlayerController>();
                playerLight = playerGameObject.GetComponent<PlayerLight>();

                if (playerController != null)
                {
                    playerStats = playerController._playerStats;
                }
                else
                {
                    Debug.LogError("PlayerController component not found on Player GameObject.");
                }
            }
            else
            {
                Debug.LogError("Player GameObject not found. Make sure your Player has the tag 'Player'.");
            }

            // ดึง Rigidbody และ NavMeshAgent ของ EnemyAI ตัวนี้
            rb = GetComponent<Rigidbody>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            // ตรวจสอบว่าได้ลากคอมโพเนนต์ที่จำเป็นเข้ามาใน Inspector หรือยัง
            if (enemyMeshRenderer == null || enemyCollider == null || cubeFormPrefab == null)
            {
                Debug.LogError("Assign Enemy Mesh Renderer, Collider, and Cube Form Prefab in the Inspector of " + gameObject.name + "!");
                enabled = false;
            }

            // ตรวจสอบว่า prefab ของกล่องมี MeshFilter และ MeshRenderer
            if (cubeFormPrefab != null && (cubeFormPrefab.GetComponent<MeshFilter>() == null || cubeFormPrefab.GetComponent<MeshRenderer>() == null))
            {
                Debug.LogWarning("Cube Form Prefab might be missing MeshFilter or MeshRenderer components. It needs them to be visible.");
            }
        }

    private void Update()
    {
        if (playerGameObject == null || playerLight == null) return;

        CheckIfHitByFlashlight();

        // *** ถ้าอยู่ในร่างกล่อง และกล่องถูกหยิบโดยผู้เล่น ให้ย้าย EnemyAI ตามกล่อง ***
        if (currentState == EnemyState.StunnedAsCube && currentCubeFormInstance != null)
        {
            // ตรวจสอบว่ากล่องนี้มี MoveableObject และกำลังถูกหยิบอยู่หรือไม่
            MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
            if (moveableObj != null && moveableObj.IsPicked()) // ต้องเพิ่ม IsPicked() ใน MoveableObject
            {
                // ทำให้ EnemyAI GameObject (Parent) ตามตำแหน่งของกล่อง
                transform.position = currentCubeFormInstance.transform.position;
                transform.rotation = currentCubeFormInstance.transform.rotation;
            }
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                break;
            case EnemyState.StunnedAsCube:
                HandleStunnedAsCubeState();
                break;
        }
    }

    // --- State Handlers ---
    private void HandleIdleState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void HandleChasingState()
    {
        // Logic การไล่ตาม Player (ทำงานเสมอใน State นี้)
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerGameObject.transform.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;

            Vector3 lookAtPlayer = playerGameObject.transform.position;
            lookAtPlayer.y = transform.position.y;
            transform.LookAt(lookAtPlayer);

            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            currentState = EnemyState.Idle;
            flashlightHitTimer = 0f;
            return;
        }

        // อัปเดต flashlightHitTimer หลังจาก Logic การเคลื่อนไหว/โจมตี
        if (isCurrentlyHitByFlashlight)
        {
            flashlightHitTimer += Time.deltaTime;
        }
        else
        {
            flashlightHitTimer = 0f;
        }

        // ตรวจสอบว่าถึงเวลาเปลี่ยนเป็นกล่องแล้วหรือยัง
        if (flashlightHitTimer >= flashlightStunDuration)
        {
            TransitionToCubeForm();
            flashlightHitTimer = 0f;
        }
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

    private void CheckIfHitByFlashlight()
    {
        isCurrentlyHitByFlashlight = false;

        if (playerLight != null && playerLight.FlashLight != null && playerLight.FlashLight.activeSelf)
        {
            Light flashlightComponent = playerLight.FlashLight.GetComponent<Light>();
            if (flashlightComponent != null && flashlightComponent.intensity > 0)
            {
                List<GameObject> objectsInLight = playerLight.GetObjectsInFlashlightCone();
                if (objectsInLight != null)
                {
                    foreach (GameObject obj in objectsInLight)
                    {
                        if (currentState != EnemyState.StunnedAsCube && obj == this.gameObject)
                        {
                            isCurrentlyHitByFlashlight = true;
                            break;
                        }
                        else if (currentState == EnemyState.StunnedAsCube && currentCubeFormInstance != null && obj == currentCubeFormInstance)
                        {
                            isCurrentlyHitByFlashlight = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void TransitionToCubeForm()
    {
        currentState = EnemyState.StunnedAsCube;

        if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        if (cubeFormPrefab != null)
        {
            // สร้างกล่องที่ตำแหน่งปัจจุบันของ EnemyAI
            currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation);
            // *** สิ่งสำคัญ: ทำให้ EnemyAI GameObject เป็น Parent ของกล่องชั่วคราว ***
            // *** เพื่อให้กล่อง (child) ควบคุมตำแหน่งของ EnemyAI (parent) ได้ง่ายขึ้น ***
            currentCubeFormInstance.transform.SetParent(this.transform);
        }
    }

    private void AttackPlayer()
    {
        if (playerController != null)
        {
            playerController.takeDamage(damage);
            lastAttackTime = Time.time;
            Debug.Log($"Enemy attacked Player for {damage} damage. Player Health: {playerStats.Health}");
        }
    }

    private void TransitionToEnemyForm()
    {
        // *** ก่อนจะกลับร่าง ให้ Unparent กล่องออกจาก EnemyAI ก่อน ***
        if (currentCubeFormInstance != null)
        {
            currentCubeFormInstance.transform.parent = null;
        }

        // ตำแหน่งของ EnemyAI (ซึ่งตอนนี้คือตำแหน่งล่าสุดของกล่อง)
        Vector3 spawnPosition = transform.position;
        float colliderHeight = enemyCollider != null ? enemyCollider.bounds.extents.y : 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 5f, Vector3.down, out hit, 10f, groundLayer))
        {
            spawnPosition = hit.point + Vector3.up * colliderHeight;
        }
        else
        {
            Debug.LogWarning($"Enemy '{gameObject.name}' couldn't find ground to respawn on! Spawning at current cube position.");
            // หากไม่เจอพื้น ก็ปล่อยให้มันอยู่ที่ตำแหน่งกล่องปัจจุบัน (อาจจะลอย)
            // หรือจะให้มันตกลงมาเองด้วย Rigidbody/Gravity ก็ได้ (ถ้าไม่เป็น Kinematic)
        }

        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
        }

        currentState = EnemyState.Idle;
        flashlightHitTimer = 0f;
        cubeFormTimer = 0f;

        if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = true;
        if (enemyCollider != null) enemyCollider.enabled = true;

        if (currentCubeFormInstance != null)
        {
            Destroy(currentCubeFormInstance);
            currentCubeFormInstance = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ตรวจสอบว่าตัวแปรเหล่านี้ถูกประกาศอยู่ในคลาส EnemyAI
        // และเมธอดนี้อยู่ภายในขอบเขตของคลาส EnemyAI โดยตรง
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red; // มี Gizmos.Color.Color.red - แก้เป็น Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}