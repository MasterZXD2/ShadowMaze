using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Video; // อาจจะไม่จำเป็น ถ้าไม่ได้ใช้ VideoPlayer

namespace ShadowMaze.InteractSystem
{
    public class MoveableObject : InteractObject
    {
        private bool pick = false;
        [SerializeField] private Rigidbody _rb;

        [field: SerializeField] private string interactLayerName = "PlrInteractObject";
        [field: SerializeField] private LayerMask raycastMask; // สำหรับ Raycast
        [field: SerializeField] private LayerMask _groundLayers; // อาจจะซ้ำกับ groundLayer ใน EnemyAI ถ้าใช้ Layer เดียวกัน

        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float height;
        [SerializeField] private float far;

        private int interactLayer;
        private int _normalLayer;

        public float Gravity = 10f; // ดูเหมือนไม่ได้ใช้
        private float _verticalVelo = 0f; // ดูเหมือนไม่ได้ใช้

        // *** เพิ่ม Public Method เพื่อตรวจสอบสถานะการหยิบ ***
        public bool IsPicked()
        {
            return pick;
        }

        protected override void Start()
        {
            base.Start();
            interactLayer = LayerMask.NameToLayer(interactLayerName);
            _normalLayer = gameObject.layer;
        }

        protected override void Update()
        {
            base.Update();
            if (pick) MoveSelectObj();
        }

        private void FixedUpdate()
        {
            if (!_rb) return;
            if (pick)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
                // *** ถ้ากล่องนี้มี Parent เป็น EnemyAI เราไม่ต้องการให้ MoveableObject เปลี่ยน Parent อีก ***
                // *** เพราะ EnemyAI จะเป็นตัว Parent ที่เราต้องการให้ตามกล่อง ***
                // *** ดังนัน้ ตรงนี้ไม่ต้อง SetParent อีก (หรือถ้าเคยมีก็เอาออก) ***
            }
            else
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
                // *** ถ้ากล่องนี้ถูกปล่อยออกจากมือผู้เล่น ให้มันไม่มี Parent (กลับไปที่โลก) ***
                if (transform.parent != null && transform.parent.GetComponent<PlayerController>() != null)
                {
                    transform.parent = null;
                }
            }
        }

        public override void Interact()
        {
            pick = !pick;
            if (pick)
            {
                gameObject.layer = interactLayer;
                // *** เมื่อหยิบ ให้กล่องเป็นลูกของ Player Controller/Camera ***
                // *** แต่ถ้า EnemyAI เป็น Parent ของกล่อง เราจะต้องจัดการตรงนี้ให้ดี ***
                // *** เนื่องจาก EnemyAI ได้ SetParent กล่องให้เป็นลูกของตัวเองแล้ว ***
                // *** โค้ดใน MoveSelectObj จะเป็นการย้ายตำแหน่งเท่านั้น ไม่ได้เปลี่ยน Parent ใน FixedUpdate แล้ว ***
            }
            else
            {
                gameObject.layer = _normalLayer;
            }
        }

        public void MoveSelectObj()
        {
            Transform player = PlayerController.Instance.transform;

            Vector3 desiredPos = (player.position + player.forward * far + Vector3.up * height) + offset;
            Vector3 origin = player.position + Vector3.up * height;
            Vector3 direction = desiredPos - origin;
            float maxDistance = Vector3.Distance(origin, desiredPos);

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, Mathf.Infinity, raycastMask))
            {
                float dist = Vector3.Distance(origin, hit.point);
                Vector3 basePos = dist > maxDistance
                    ? origin + direction.normalized * maxDistance : hit.point;

                Vector3 fPos = basePos + hit.normal * 0.1f;
                // *** ใช้ MovePosition สำหรับ Rigidbody เพื่อการเคลื่อนที่ที่ถูกต้องกับ Physics ***
                if (_rb != null)
                {
                    _rb.MovePosition(fPos);
                }
                else
                {
                    transform.position = fPos;
                }
            }
            else
            {
                if (_rb != null)
                {
                    _rb.MovePosition(desiredPos);
                }
                else
                {
                    transform.position = desiredPos;
                }
            }
        }
    }
}