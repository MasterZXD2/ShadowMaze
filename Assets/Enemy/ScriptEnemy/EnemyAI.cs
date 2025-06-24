using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShadowMaze.MyCharacterController; // อย่าลืมเพิ่ม namespace นี้

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float damage = 10f; // ดาเมจที่ศัตรูทำต่อผู้เล่น
    [SerializeField] private float detectionRange = 5f; // รัศมีการตรวจจับ Player
    [SerializeField] private float moveSpeed = 3f; // ความเร็วในการเคลื่อนที่ของศัตรู
    [SerializeField] private float attackRange = 1.5f; // ระยะที่ศัตรูจะเริ่มโจมตี Player
    [SerializeField] private float attackCooldown = 1f; // Cooldown ในการโจมตี
    private float lastAttackTime;

    [Header("Transformation Settings")]
    [SerializeField] private float flashlightStunDuration = 3f; // ระยะเวลาที่ต้องโดนไฟฉายส่องก่อนกลายเป็นกล่อง
    [SerializeField] private float cubeFormDuration = 5f; // ระยะเวลาที่อยู่ในร่างกล่องก่อนกลับเป็นศัตรู
    [SerializeField] private MeshRenderer enemyMeshRenderer; // MeshRenderer ของ Capsule (ศัตรู)
    [SerializeField] private Collider enemyCollider; // Collider ของ Capsule (ศัตรู)
    [SerializeField] private GameObject cubeFormPrefab; // Prefab ของ Object3D อื่นๆ ที่จะใช้เป็นกล่อง

    private GameObject playerGameObject; // อ้างอิง Player GameObject
    private PlayerController playerController; // อ้างอิง PlayerController สำหรับเรียก takeDamage
    private PlayerStats playerStats; // อ้างอิง PlayerStats สำหรับดู Health
    private PlayerLight playerLight; // อ้างอิง PlayerLight เพื่อตรวจจับไฟฉาย

    private GameObject currentCubeFormInstance; // Instance ของกล่องที่สร้างขึ้น
    private bool isHitByFlashlight = false; // สถานะว่าศัตรูกำลังโดนไฟฉายส่องหรือไม่
    private float flashlightHitTimer = 0f; // นับเวลาที่โดนไฟฉายส่อง
    private float cubeFormTimer = 0f; // นับเวลาที่อยู่ในร่างกล่อง

    private enum EnemyState
    {
        Idle,
        Chasing,
        StunnedAsCube
    }

    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        // ค้นหา Player GameObject ด้วย Tag "Player"
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            // ดึง PlayerController และ PlayerLight จาก Player GameObject
            playerController = playerGameObject.GetComponent<PlayerController>();
            playerLight = playerGameObject.GetComponent<PlayerLight>();

            // ดึง PlayerStats จาก PlayerController
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

        // ตรวจสอบว่าได้ลากคอมโพเนนต์ที่จำเป็นเข้ามาใน Inspector หรือยัง
        if (enemyMeshRenderer == null || enemyCollider == null || cubeFormPrefab == null)
        {
            Debug.LogError("Assign Enemy Mesh Renderer, Collider, and Cube Form Prefab in the Inspector of " + gameObject.name + "!");
            enabled = false; // ปิด Script ถ้าไม่ได้ตั้งค่าที่จำเป็น
        }

        // ตรวจสอบว่า prefab มี MeshFilter และ MeshRenderer
        if (cubeFormPrefab != null && (cubeFormPrefab.GetComponent<MeshFilter>() == null || cubeFormPrefab.GetComponent<MeshRenderer>() == null))
        {
            Debug.LogWarning("Cube Form Prefab might be missing MeshFilter or MeshRenderer components. It needs them to be visible.");
        }
    }

    private void Update()
    {
        // ตรวจสอบว่า Player GameObject และ PlayerLight ถูกอ้างอิงถูกต้อง
        if (playerGameObject == null || playerLight == null) return;

        // จัดการการตรวจจับไฟฉายก่อน Logic ของ State
        HandleFlashlightInteraction();

        // Logic การทำงานตาม State ปัจจุบัน
        switch (currentState)
        {
            case EnemyState.Idle:
                CheckForPlayer();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.StunnedAsCube:
                // ในสถานะนี้ Logic การกลับร่างจะถูกจัดการใน HandleFlashlightInteraction()
                // ไม่ต้องทำอะไรเพิ่มเติมใน Update สำหรับ state นี้
                break;
        }
    }

    // --- ส่วนจัดการการโต้ตอบกับไฟฉาย ---
    private void HandleFlashlightInteraction()
    {
        // ตรวจสอบว่า PlayerLight กำลังทำงานอยู่ และไฟฉายเปิดอยู่
        if (playerLight == null || !playerLight.FlashLight.activeSelf) // ตรวจสอบ activeSelf เพื่อความมั่นใจว่าไฟฉายเปิดอยู่
        {
            isHitByFlashlight = false; // ถ้าไม่มี PlayerLight หรือไฟฉายปิดอยู่ ก็ถือว่าไม่โดนส่อง
        }
        else
        {
            // ดึงรายการวัตถุที่อยู่ในกรวยไฟฉาย
            List<GameObject> objectsInLight = playerLight.GetObjectsInFlashlightCone();

            isHitByFlashlight = false;
            if (objectsInLight != null)
            {
                // ตรวจสอบว่า GameObject ของ Enemy ตัวนี้อยู่ในรายการที่โดนไฟฉายส่องหรือไม่
                foreach (GameObject obj in objectsInLight)
                {
                    if (obj == this.gameObject) // ถ้าพบตัวเองในรายการ
                    {
                        isHitByFlashlight = true;
                        break; // ออกจาก loop เมื่อเจอ
                    }
                }
            }
        }

        // Logic สำหรับการเปลี่ยนร่างเป็นกล่องเมื่อโดนไฟฉายส่อง
        if (isHitByFlashlight)
        {
            flashlightHitTimer += Time.deltaTime;
            // ถ้าโดนส่องครบ 3 วินาที และไม่ได้อยู่ในร่างกล่องอยู่แล้ว ให้เปลี่ยนเป็นกล่อง
            if (flashlightHitTimer >= flashlightStunDuration && currentState != EnemyState.StunnedAsCube)
            {
                TransitionToCubeForm();
            }
        }
        else // ถ้าไม่โดนไฟฉายส่อง
        {
            flashlightHitTimer = 0f; // รีเซ็ตตัวจับเวลาโดนไฟฉาย
            // ถ้าศัตรูอยู่ในร่างกล่องอยู่แล้ว และไม่โดนไฟฉายส่อง ให้เริ่มนับถอยหลังกลับร่างเดิม
            if (currentState == EnemyState.StunnedAsCube)
            {
                cubeFormTimer += Time.deltaTime;
                if (cubeFormTimer >= cubeFormDuration)
                {
                    TransitionToEnemyForm();
                }
            }
            else
            {
                cubeFormTimer = 0f; // รีเซ็ต timer หากไม่ได้อยู่ในร่างกล่อง
            }
        }
    }

    // --- ส่วนจัดการพฤติกรรมศัตรู ---
    private void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerGameObject.transform.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;

            // ทำให้ศัตรูหันหน้าเข้าหาผู้เล่น (เฉพาะแกน Y)
            Vector3 lookAtPlayer = playerGameObject.transform.position;
            lookAtPlayer.y = transform.position.y; // รักษาระดับความสูงของศัตรู
            transform.LookAt(lookAtPlayer);

            // ตรวจจับการโจมตีเมื่อเข้าใกล้พอ
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // ถ้า Player ออกนอกระยะตรวจจับ ให้กลับไปสถานะ Idle
            currentState = EnemyState.Idle;
        }
    }

    private void AttackPlayer()
    {
        // เรียกใช้เมธอด takeDamage จาก PlayerController
        if (playerController != null)
        {
            playerController.takeDamage(damage);
            lastAttackTime = Time.time;
            Debug.Log($"Enemy attacked Player for {damage} damage. Player Health: {playerStats.Health}");
        }
    }

    // --- ส่วนจัดการการแปลงร่าง ---
    private void TransitionToCubeForm()
    {
        currentState = EnemyState.StunnedAsCube;
        flashlightHitTimer = 0f; // รีเซ็ต timer เมื่อเปลี่ยนสถานะ
        cubeFormTimer = 0f; // เริ่มนับเวลาสำหรับร่างกล่อง

        // ซ่อน MeshRenderer และ Collider ของศัตรู (Capsule)
        if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        // สร้าง Object3D อื่นๆ (Cube) ที่ตำแหน่งเดียวกัน
        if (cubeFormPrefab != null)
        {
            currentCubeFormInstance = Instantiate(cubeFormPrefab, transform.position, transform.rotation);
            // ทำให้กล่องเป็นลูกของ EnemyAI GameObject เพื่อการจัดการที่ง่ายขึ้น
            currentCubeFormInstance.transform.SetParent(this.transform);

            // คุณอาจจะต้องปรับ Rigidbody ของกล่องให้เป็น Kinematic เพื่อให้มันนิ่งอยู่กับที่
            Rigidbody cubeRigidbody = currentCubeFormInstance.GetComponent<Rigidbody>();
            if (cubeRigidbody != null)
            {
                cubeRigidbody.isKinematic = true;
            }
            // ถ้ามี Collider บน cube prefab ก็ควรปิดมันถ้าไม่ต้องการให้ชนกับอะไรเลย
            // หรือถ้าจะให้ชนก็แล้วแต่การออกแบบเกม
        }
    }

    private void TransitionToEnemyForm()
    {
        currentState = EnemyState.Idle; // กลับสู่สถานะ Idle (หรือจะให้เป็น Chasing ทันทีก็ได้ถ้า Player อยู่ในระยะ)
        flashlightHitTimer = 0f; // รีเซ็ต timer เมื่อกลับร่างเดิม
        cubeFormTimer = 0f; // รีเซ็ต timer

        // แสดง MeshRenderer และ Collider ของศัตรู (Capsule) กลับมา
        if (enemyMeshRenderer != null) enemyMeshRenderer.enabled = true;
        if (enemyCollider != null) enemyCollider.enabled = true;

        // ทำลาย instance ของกล่อง
        if (currentCubeFormInstance != null)
        {
            Destroy(currentCubeFormInstance);
            currentCubeFormInstance = null;
        }
    }

    // --- สำหรับแสดง Debug ใน Scene View ---
    private void OnDrawGizmosSelected()
    {
        // วาด Sphere สำหรับ Detection Range ใน Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // วาด Sphere สำหรับ Attack Range ใน Scene View
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}