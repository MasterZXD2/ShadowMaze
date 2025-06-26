using ShadowMaze.MyCharacterController;
using UnityEngine;
// using UnityEngine.Video; // ลบถ้าไม่ได้ใช้

namespace ShadowMaze.InteractSystem
{
    public class MoveableObject : InteractObject
    {
        private bool pick = false;
        [SerializeField] private Rigidbody _rb;

        [field: SerializeField] private string interactLayerName = "PlrInteractObject";
        [field: SerializeField] private LayerMask raycastMask;

        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float height;
        [SerializeField] private float far;

        private int interactLayer;
        private int _normalLayer;

        // ตัวแปรที่ใช้สำหรับ EnemyAI เพื่อตรวจสอบสถานะการหยิบ
        public bool IsPicked()
        {
            return pick;
        }

        protected override void Start()
        {
            base.Start();
            interactLayer = LayerMask.NameToLayer(interactLayerName);
            _normalLayer = gameObject.layer;

            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
                if (_rb == null)
                {
                    Debug.LogError($"MoveableObject on {gameObject.name}: Rigidbody not assigned and not found on this GameObject. Physics movement will not work.");
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_rb) return;

            if (pick)
            {
                // *** สำคัญ: ทำให้ Rigidbody เป็น Kinematic และปิด Gravity ทันทีเมื่อถูกหยิบ ***
                // ตั้งค่าเหล่านี้เพื่อให้ Rigidbody ถูกควบคุมด้วยมือได้อย่างราบรื่น
                if (!_rb.isKinematic) _rb.isKinematic = true;
                if (_rb.useGravity) _rb.useGravity = false;

                // *** ตั้ง Parent เป็น Player's objectHolder เมื่อถูกหยิบ ***
                // ต้องแน่ใจว่า PlayerController.Instance.objectHolder มีอยู่จริง
                if (PlayerController.Instance != null && PlayerController.Instance.objectHolder != null && transform.parent != PlayerController.Instance.objectHolder)
                {
                    transform.SetParent(PlayerController.Instance.objectHolder);
                    _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // อาจช่วยลดปัญหา collision ขณะถือ
                }

                // เรียก MoveSelectObj() เพื่อคำนวณและตั้งค่าตำแหน่ง
                MoveSelectObj();
            }
            else
            {
                // *** สำคัญ: คืนค่า Rigidbody ปกติเมื่อปล่อย ***
                if (_rb.isKinematic) _rb.isKinematic = false;
                if (!_rb.useGravity) _rb.useGravity = true;
                _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // กลับไปใช้โหมดปกติเพื่อการชนที่ดีขึ้น

                // *** Unparent จาก Player เมื่อถูกปล่อย ***
                if (transform.parent != null && transform.parent.GetComponent<PlayerController>() != null) // ตรวจสอบว่า Parent คือ Player (หรือ objectHolder)
                {
                    transform.parent = null; // Unparent ออกจาก Player
                }
            }
        }

        public override void Interact()
        {
            pick = !pick;
            if (pick)
            {
                gameObject.layer = interactLayer;
                // Debug.Log($"MoveableObject {gameObject.name} picked. Layer set to {LayerMask.LayerToName(interactLayer)}");
                // การ SetParent จะเกิดขึ้นใน FixedUpdate()
            }
            else
            {
                gameObject.layer = _normalLayer;
                // Debug.Log($"MoveableObject {gameObject.name} dropped. Layer set to {LayerMask.LayerToName(_normalLayer)}");
                // การ Unparent จะเกิดขึ้นใน FixedUpdate()
            }
        }

        public void MoveSelectObj()
        {
            if (PlayerController.Instance == null)
            {
                Debug.LogWarning("PlayerController.Instance is null in MoveableObject.MoveSelectObj(). Cannot move object.");
                return;
            }

            Transform player = PlayerController.Instance.transform;

            // คำนวณตำแหน่งเป้าหมายตาม Player และ offset/height/far
            Vector3 desiredPos = (player.position + player.forward * far + Vector3.up * height) + offset;

            // ใช้ตำแหน่งของ objectHolder ของ Player เป็นจุดเริ่มต้น Raycast เพื่อความแม่นยำสูงสุด
            Vector3 raycastOrigin = PlayerController.Instance.objectHolder != null ? PlayerController.Instance.objectHolder.position : player.position + Vector3.up * height;
            Vector3 direction = desiredPos - raycastOrigin;
            float maxDistance = Vector3.Distance(raycastOrigin, desiredPos);

            RaycastHit hit;
            if (Physics.Raycast(raycastOrigin, direction.normalized, out hit, maxDistance, raycastMask, QueryTriggerInteraction.Ignore))
            {
                // ถ้าชนกับสิ่งกีดขวาง ให้วางวัตถุที่จุดชน แต่ดันออกมาเล็กน้อยเพื่อป้องกันการซ้อนทับ
                Vector3 fPos = hit.point + hit.normal * 0.05f;
                _rb.position = fPos;
            }
            else
            {
                // ถ้าไม่ชน ให้วางวัตถุที่ตำแหน่งเป้าหมาย
                _rb.position = desiredPos;
            }
            _rb.rotation = Quaternion.LookRotation(player.forward); // ทำให้กล่องหันหน้าตาม Player ด้วย
        }
    }
}