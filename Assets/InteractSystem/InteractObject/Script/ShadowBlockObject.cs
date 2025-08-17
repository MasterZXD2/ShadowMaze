using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using UnityEngine;

public class ShadowBlockObject : BookPuzzleObject
{
    public ShadowPuzzle SP;
    [SerializeField] private BoxCollider collider;

    protected override void Start()
    {
        base.Start();
        collider = GetComponent<BoxCollider>();
    }

    protected override void LateUpdate()
    {
        
    }

    public override void HitByFlashlight()
    {
        if (SP) b = true;
        else b = false;
    }

    protected override void MoveSelectObj()
    {
        if (PlayerController.Instance == null) return;
        PlayerController _player = PlayerController.Instance;
        Transform player = _player.transform;
        if (!player) return;

        _scrollDist += _player._playerLocomotionInput.ScrollInput.y / 10f;
        _scrollDist = Mathf.Clamp(_scrollDist, MinScrollDist, MaxScrollDist);
        range = _currrange + _scrollDist;

        //Quaternion cameraYaw = Quaternion.Euler(_player._camRotation.y, _player._camRotation.x, 0f) * _rotation; // yaw only

        // ตำแหน่งที่ต้องการให้อยู่ (หน้าผู้เล่น + ยกสูง)
        Vector3 targetPosition = (player.position + _player._plrCamera.transform.forward * far + Vector3.up * height) + offset;

        // ใช้ตำแหน่งของ objectHolder ของ Player เป็นจุดเริ่มต้น Raycast เพื่อความแม่นยำสูงสุด
        Vector3 raycastOrigin = player.position + _player._plrCamera.transform.forward + Vector3.up * height;
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
        //transform.rotation = Quaternion.Slerp(transform.rotation, cameraYaw, Speed * Time.deltaTime);
        transform.rotation = player.rotation * _rotation;
        transform.LookAt(Camera.main.transform);
    }
}
