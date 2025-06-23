using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Video;

namespace ShadowMaze.InteractSystem
{
    public class MoveableObject : InteractObject
    {
        private bool pick = false;
        [SerializeField] private Rigidbody _rb;

        [field: SerializeField] private string interactLayerName = "PlrInteractObject";
        [field: SerializeField] private LayerMask raycastMask; // สำหรับ Raycast
        [field: SerializeField] private LayerMask _groundLayers;
        
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float height;
        [SerializeField]  private float far;

        private int interactLayer;
        private int _normalLayer;

        public float Gravity = 10f;
        private float _verticalVelo = 0f;

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
            } else
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
            }
        }

        public override void Interact()
        {
            pick = !pick;
            if (pick) gameObject.layer = interactLayer;
            else gameObject.layer = _normalLayer;
        }

        public void MoveSelectObj()
        {
            Transform player = PlayerController.Instance.transform;

            // ค่าตำแหน่งเป้าหมายด้านหน้าผู้เล่น
            Vector3 desiredPos = (player.position + player.forward * far + Vector3.up * height) + offset;

            // ตำแหน่งเริ่มต้นคือหัวของผู้เล่น (หรือเลื่อนขึ้นจากตำแหน่งล่าง)
            Vector3 origin = player.position + Vector3.up * height;

            Vector3 direction = desiredPos - origin;
            float maxDistance = Vector3.Distance(origin, desiredPos);

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, Mathf.Infinity, raycastMask))
            {
                float dist = Vector3.Distance(origin, hit.point);
                // ถ้า hit ระยะไกลเกิน MaxDistance หรือไม่ ก็หา basePos ก่อน
                Vector3 basePos = dist > maxDistance
                    ? origin + direction.normalized * maxDistance : hit.point;

                Vector3 fPos = basePos + hit.normal * 0.1f;
                // offset ตัว spawn ให้พ้น collider ของ hit
                transform.position = fPos;
            }
            else
            {
                transform.position = desiredPos;
            }
        }
    }
}