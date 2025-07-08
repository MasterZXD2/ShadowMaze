using ShadowMaze.MyCharacterController;
using UnityEngine;
using static UnityEngine.UI.Image;
// using UnityEngine.Video; // ลบถ้าไม่ได้ใช้

namespace ShadowMaze.InteractSystem
{
    public class MoveableObject : InteractObject
    {
        private bool pick = false;
        [SerializeField] private Rigidbody _rb;

        [field: SerializeField] private string interactLayerName = "PlrInteractObject";
        [field: SerializeField] private LayerMask raycastMask;

        [SerializeField] private Quaternion _rotation;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float height;
        [SerializeField] private float far;

        private int interactLayer;
        private int _normalLayer;

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

        protected override void Update()
        {
            base.Update();
            if (pick) MoveSelectObj();
        }

        public override void Interact()
        {
            pick = !pick;
            if (pick)
            {
                gameObject.layer = interactLayer;
                _rb.isKinematic = true;
                _rb.useGravity = false;
                // Debug.Log($"MoveableObject {gameObject.name} picked. Layer set to {LayerMask.LayerToName(interactLayer)}");
                // การ SetParent จะเกิดขึ้นใน FixedUpdate()
            }
            else
            {
                gameObject.layer = _normalLayer;
                _rb.isKinematic = false;
                _rb.useGravity = true;
                // Debug.Log($"MoveableObject {gameObject.name} dropped. Layer set to {LayerMask.LayerToName(_normalLayer)}");
                // การ Unparent จะเกิดขึ้นใน FixedUpdate()
            }
        }

        public void MoveSelectObj()
        {
            if (PlayerController.Instance == null) return;

            Transform player = PlayerController.Instance.transform;

            // คำนวณตำแหน่งเป้าหมายตาม Player และ offset/height/far
            Vector3 desiredPos = (player.position + player.forward * far + Vector3.up * height) + offset;

            // ใช้ตำแหน่งของ objectHolder ของ Player เป็นจุดเริ่มต้น Raycast เพื่อความแม่นยำสูงสุด
            Vector3 raycastOrigin = player.position + Vector3.up * height;
            Vector3 direction = desiredPos - raycastOrigin;
            float maxDistance = Vector3.Distance(raycastOrigin, desiredPos);

            if (Physics.Raycast(raycastOrigin, direction.normalized, out RaycastHit hit, Mathf.Infinity, raycastMask))
            {
                float dist = Vector3.Distance(raycastOrigin, hit.point);

                Vector3 basePos = dist > maxDistance
                    ? raycastOrigin + direction.normalized * maxDistance : hit.point;

                Vector3 fPos = basePos + hit.normal * 0.1f;

                transform.position = fPos;
            }
            else
            {
                transform.position = desiredPos;
            }
            transform.rotation = player.rotation * _rotation;
        }
    }
}