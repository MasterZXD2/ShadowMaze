using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShadowMaze.MyCharacterController;
using ShadowMaze.InteractSystem;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // --- ส่วนหัวข้อ "Enemy Stats" (ค่าสถานะศัตรู) ---
    [Header("Enemy Stats")]
    [SerializeField] private float damage = 10f; // ดาเมจที่ศัตรูทำต่อผู้เล่น
    [SerializeField] private float detectionRange = 5f; // รัศมีการตรวจจับ Player
    [SerializeField] private float moveSpeed = 3f; // ความเร็วในการเคลื่อนที่ของศัตรู
    [SerializeField] private float attackRange = 1.5f; // ระยะที่ศัตรูจะเริ่มโจมตี Player
    [SerializeField] private float attackCooldown = 1f; // Cooldown ในการโจมตี
    private float lastAttackTime; // เวลาที่โจมตีครั้งล่าสุด

    // --- ส่วนหัวข้อ "Transformation Settings" (การตั้งค่าการแปลงร่าง) ---
    [Header("Transformation Settings")]
    [SerializeField] private float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง (ตั้งเป็น 0 จะเปลี่ยนทันที)
    [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู (ถ้าไม่โดนไฟฉายส่อง)
    [SerializeField] private MeshRenderer enemyMeshRenderer; // MeshRenderer ของแคปซูล (ศัตรู)
    [SerializeField] private Collider enemyCollider; // Collider ของแคปซูล (ศัตรู)
    [SerializeField] private GameObject cubeFormPrefab; // Prefab ของกล่องที่จะใช้แปลงร่าง
    [SerializeField] private LayerMask groundLayer; // Layer ของพื้นสำหรับตรวจสอบการวางตำแหน่งเมื่อกลับร่าง

    // --- การอ้างอิงส่วนตัว (อ้างอิงถึง Component อื่นๆ) ---
    private GameObject playerGameObject; // อ้างอิงถึง Player GameObject
    private PlayerController playerController; // อ้างอิงถึง PlayerController script
    private PlayerStats playerStats; // อ้างอิงถึง PlayerStats (จาก PlayerController)
    private PlayerLight playerLight; // อ้างอิงถึง PlayerLight script
    private Rigidbody rb; // Rigidbody ของ EnemyAI GameObject ตัวนี้ (แคปซูล)
    private NavMeshAgent navMeshAgent; // NavMeshAgent ของ EnemyAI GameObject ตัวนี้ (แคปซูล) (ถ้ามี)

    private GameObject currentCubeFormInstance; // ตัวแปรเก็บ Instance ของกล่องที่ถูกสร้างขึ้นมา

    // --- ตัวแปรสถานะ ---
    private bool isCurrentlyHitByFlashlight = false; // สถานะปัจจุบันว่ากำลังโดนไฟฉายส่องหรือไม่
    private float flashlightHitTimer = 0f; // นับเวลาที่โดนไฟฉายส่อง (เพื่อเปลี่ยนเป็นกล่อง)
    private float cubeFormTimer = 0f; // นับเวลาที่อยู่ในร่างกล่อง (เพื่อกลับร่างเดิม)

    // --- สถานะของศัตรู (Enum) ---
    private enum EnemyState
    {
        Idle,           // นิ่งๆ ไม่ทำอะไร
        Chasing,        // กำลังไล่ล่าผู้เล่น
        StunnedAsCube   // ถูกสตันอยู่ในร่างกล่อง
    }
    private EnemyState currentState = EnemyState.Idle; // กำหนดสถานะเริ่มต้นเป็น Idle

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
                Debug.LogError("EnemyAI: PlayerController component not found on Player GameObject.");
            }
        }
        else
        {
            Debug.LogError("EnemyAI: Player GameObject not found. Make sure your Player has the tag 'Player'.");
        }

        // ดึง Rigidbody และ NavMeshAgent จาก GameObject ตัวนี้ (EnemyAI)
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // ตรวจสอบว่าได้ลากคอมโพเนนต์ที่จำเป็นเข้ามาใน Inspector หรือยัง
        if (enemyMeshRenderer == null || enemyCollider == null || cubeFormPrefab == null)
        {
            Debug.LogError("EnemyAI: Assign Enemy Mesh Renderer, Collider, and Cube Form Prefab in the Inspector of " + gameObject.name + "!");
            enabled = false; // ปิด Script ถ้าไม่ได้ตั้งค่าที่จำเป็น
        }

        // ตรวจสอบว่า prefab ของกล่องมี MeshFilter และ MeshRenderer
        if (cubeFormPrefab != null && (cubeFormPrefab.GetComponent<MeshFilter>() == null || cubeFormPrefab.GetComponent<MeshRenderer>() == null))
        {
            Debug.LogWarning("EnemyAI: Cube Form Prefab might be missing MeshFilter or MeshRenderer components. It needs them to be visible.");
        }
    }

    private void Update()
    {
        // หยุดการทำงานถ้าไม่พบ Player หรือ PlayerLight
        if (playerGameObject == null || playerLight == null) return;

        // ตรวจสอบว่าศัตรูกำลังโดนไฟฉายส่องอยู่หรือไม่ใน Frame นี้
        CheckIfHitByFlashlight();

        // Logic การทำงานตาม State ปัจจุบัน
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

    // FixedUpdate สำหรับการเคลื่อนย้าย Rigidbody ของ EnemyAI (ถ้าจำเป็น)
    // ในกรณีนี้ EnemyAI ไม่ได้ถูกหยิบโดยตรง แต่จะตามตำแหน่งของกล่องใน FixedUpdate ถ้ากล่องถูกหยิบ
    private void FixedUpdate()
    {
        if (currentState == EnemyState.StunnedAsCube && currentCubeFormInstance != null)
        {
            MoveableObject moveableObj = currentCubeFormInstance.GetComponent<MoveableObject>();
            if (moveableObj != null && moveableObj.IsPicked())
            {
                // หากกล่องถูกหยิบ EnemyAI จะตามตำแหน่งของกล่อง (ซึ่งตอนนี้อาจจะเป็น Child ของ Player)
                // การตั้งค่า rb.isKinematic เป็น true ใน TransitionToCubeForm() ช่วยให้ EnemyAI สามารถถูกควบคุมโดยตรงได้
                if (rb != null)
                {
                    rb.position = currentCubeFormInstance.transform.position;
                    rb.rotation = currentCubeFormInstance.transform.rotation;
                }
                else
                {
                    // Fallback ถ้าไม่มี Rigidbody (ซึ่งไม่ควรเกิดขึ้นถ้ามี Collider)
                    transform.position = currentCubeFormInstance.transform.position;
                    transform.rotation = currentCubeFormInstance.transform.rotation;
                }
            }
        }
    }


    // --- Main Logic Methods ---

    // ตรวจสอบว่าศัตรูกำลังโดนไฟฉายส่องอยู่หรือไม่ใน Frame ปัจจุบัน
    private void CheckIfHitByFlashlight()
    {
        isCurrentlyHitByFlashlight = false; // ตั้งค่าเริ่มต้นเป็น false ในทุก Frame

        // ตรวจสอบว่า PlayerLight กำลังทำงานอยู่ และไฟฉายเปิดอยู่ด้วย
        if (playerLight != null && playerLight.FlashLight != null && playerLight.FlashLight.activeSelf)
        {
            // ตรวจสอบความสว่างของไฟฉายด้วย
            Light flashlightComponent = playerLight.FlashLight.GetComponent<Light>();
            if (flashlightComponent != null && flashlightComponent.intensity > 0)
            {
                List<GameObject> objectsInLight = playerLight.GetObjectsInFlashlightCone();
                if (objectsInLight != null)
                {
                    foreach (GameObject obj in objectsInLight)
                    {
                        // ถ้าอยู่ในร่างปกติ: ตรวจสอบว่า Raycast ชนกับ EnemyAI GameObject (ตัวแคปซูลหลัก)
                        if (currentState != EnemyState.StunnedAsCube && obj == this.gameObject)
                        {
                            isCurrentlyHitByFlashlight = true;
                            break;
                        }
                        // ถ้าอยู่ในร่างกล่อง: ตรวจสอบว่า Raycast ชนกับ CubeFormInstance (ตัวกล่องที่สร้างขึ้นมา)
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

    // --- State Handlers (จัดการแต่ละสถานะ) ---
    private void HandleIdleState()
    {
        // ในสถานะ Idle, จะเริ่มไล่ล่าเมื่อผู้เล่นเข้าใกล้ detectionRange
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void HandleChasingState()
    {
        // Logic การไล่ตาม Player
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerGameObject.transform.position - transform.position).normalized;
            // ใช้ Rigidbody.MovePosition ถ้ามี NavMeshAgent ปิดอยู่
            if (navMeshAgent != null && !navMeshAgent.enabled && rb != null && !rb.isKinematic)
            {
                rb.MovePosition(transform.position + directionToPlayer * moveSpeed * Time.deltaTime);
            }
            else // ถ้าไม่มี Rigidbody หรือ Rigidbody เป็น kinematic หรือ NavMeshAgent ทำงานอยู่
            {
                transform.position += directionToPlayer * moveSpeed * Time.deltaTime; // Fallback หรือใช้ NavMeshAgent แทน
            }

            Vector3 lookAtPlayer = playerGameObject.transform.position;
            lookAtPlayer.y = transform.position.y; // ทำให้ศัตรูหันเฉพาะแกน Y
            transform.LookAt(lookAtPlayer); // หันหน้าเข้าหาผู้เล่น

            // โจมตีผู้เล่นถ้าอยู่ในระยะ attackRange และ Cooldown หมดแล้ว
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // ถ้าผู้เล่นออกนอกระยะ detectionRange ให้กลับสู่สถานะ Idle
            currentState = EnemyState.Idle;
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

    // --- ส่วนการโจมตี ---
    private void AttackPlayer()
    {
        if (playerController != null)
        {
            playerController.takeDamage(damage); // เรียกใช้เมธอด takeDamage ของ Player
            lastAttackTime = Time.time; // บันทึกเวลาที่โจมตีล่าสุด
            Debug.Log($"Enemy attacked Player for {damage} damage. Player Health: {playerStats.Health}");
        }
    }

    // --- ส่วนการแปลงร่าง ---
    private void TransitionToCubeForm()
    {
        currentState = EnemyState.StunnedAsCube; // เปลี่ยนสถานะเป็น StunnedAsCube

        if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false; // ซ่อน Mesh ของแคปซูล
        if (enemyCollider != null) enemyCollider.enabled = false; // ปิด Collider ของแคปซูล

        // ทำให้ Rigidbody ของ EnemyAI ตัวนี้เป็น Kinematic และหยุด NavMeshAgent
        // เพื่อไม่ให้มันได้รับผลกระทบจากฟิสิกส์ หรือเคลื่อนที่เองขณะเป็นกล่อง
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero; // หยุดการเคลื่อนที่ที่เหลืออยู่
            rb.angularVelocity = Vector3.zero; // หยุดการหมุนที่เหลืออยู่
        }
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true; // สั่งหยุด NavMeshAgent
            navMeshAgent.enabled = false; // ปิด NavMeshAgent ชั่วคราว
        }

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

        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, 15f, groundLayer))
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
        currentState = EnemyState.Idle;
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
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // วาดวงกลมสีแดงแสดงรัศมีการโจมตีผู้เล่น (attackRange)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}