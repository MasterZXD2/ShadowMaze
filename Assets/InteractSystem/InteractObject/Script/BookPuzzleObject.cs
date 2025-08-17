using ShadowMaze.MyCharacterController;
using Unity.VisualScripting;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class BookPuzzleObject : MoveableObject
    {
        public string Name;
        [SerializeField] private Light light;
        [SerializeField] protected float MaxScrollDist = 2f;
        [SerializeField] protected float MinScrollDist = -.2f;
        [SerializeField] protected float Speed = 5f;

        protected bool b;
        protected float _currrange;
        protected float _scrollDist;

        protected override void Start()
        {
            base.Start();
            _currrange = range;
        }

        protected override void Update()
        {
            base.Update();
            if (light) light.enabled = b;
            b = false;
        }

        protected virtual void LateUpdate()
        {
            //b = false;
        }

        public override void Interact()
        {
            base.Interact();
            _scrollDist = far;
            range = _currrange;
        }

        protected override void MoveSelectObj()
        {
            PlayerController player = PlayerController.Instance;
            if (!player) return;

            _scrollDist += player._playerLocomotionInput.ScrollInput.y / 10f;
            _scrollDist = Mathf.Clamp(_scrollDist, MinScrollDist, MaxScrollDist);
            range = _currrange + _scrollDist;

            Quaternion cameraYaw = Quaternion.Euler(player._camRotation.y, player._camRotation.x, 0f) * _rotation; // yaw only

            // ตำแหน่งที่ต้องการให้อยู่ (หน้าผู้เล่น + ยกสูง)
            Vector3 targetPosition = (player.transform.position + player._plrCamera.transform.forward * far + Vector3.up * height) + offset;

            // ใช้ตำแหน่งของ objectHolder ของ Player เป็นจุดเริ่มต้น Raycast เพื่อความแม่นยำสูงสุด
            Vector3 raycastOrigin = player.transform.position + player._plrCamera.transform.forward + Vector3.up * height;
            Vector3 direction = targetPosition - raycastOrigin;

            if (Physics.Raycast(raycastOrigin, direction.normalized, out RaycastHit hit, Mathf.Infinity, raycastMask))
            {
                float dist = Vector3.Distance(raycastOrigin, hit.point);

                Vector3 basePos = dist > _scrollDist
                    ? raycastOrigin + direction.normalized * _scrollDist : hit.point;

                Vector3 fPos = basePos + hit.normal * 0.1f;

                transform.position = fPos;
            }
            else
            {
                transform.position = raycastOrigin + direction.normalized * _scrollDist;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraYaw, Speed * Time.deltaTime);
        }

        public virtual void HitByFlashlight() {
            b = true;
        }

        public bool getB()
        {
            return b;
        }
    }
}
