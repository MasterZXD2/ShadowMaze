using ShadowMaze.MyCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace ShadowMaze.InteractSystem 
{
    enum axisMirror { 
        X,
        Y,
        Z
    }
    public class InteractObject : MonoBehaviour
    {
        [SerializeField] protected GameObject UI;
        [SerializeField] protected float range = 2f;
        [field: SerializeField] private axisMirror axis = axisMirror.Z;
        protected GameObject currUI;
        protected bool _playerIsInRange = false;
        public string _name = "null";

        protected virtual void Start()
        {
            if (!UI) return;
            currUI = Instantiate(UI, transform);
            currUI.transform.localPosition = Vector3.zero;
            
            //Vector3 scale = currUI.transform.localScale;
            //scale.x = 1 + (1 - transform.localScale.x);
            currUI.transform.localScale = Vector3.one;

            Canvas canvas = currUI.GetComponentInChildren<Canvas>();

            Vector3 uiOffset = -transform.forward;
            if (axis == axisMirror.X) uiOffset *= ((transform.localScale.x / 2f) + 0.35f);
            else if (axis == axisMirror.Y) uiOffset *= ((transform.localScale.y / 2f) + 0.35f);
            else uiOffset *= ((transform.localScale.z / 2f) + 0.35f);

            canvas.transform.position = transform.position + uiOffset;

            currUI.SetActive(false);
        }

        protected virtual void Update()
        {
            if (!UI) return;
            PlayerController player = PlayerController.Instance;
            if (!player || !player._playerInteract) return;
            if (player.astop) return;

            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;

            if (sqrDist < range * range && CanPlayerSee(player.transform))
            {
                 if (!_playerIsInRange && !player._playerInteract._interactObject)
                 {
                    player._playerInteract.SetInteractObject(this, currUI);
                    _playerIsInRange = true;
                    currUI.SetActive(true);
                 }
            }
            else
            {
                if (_playerIsInRange && player._playerInteract._interactObject == this)
                {
                    player._playerInteract.RemoveInteractObject();
                    _playerIsInRange = false;
                    currUI.SetActive(false);
                }
            }
        }

        private bool CanPlayerSee(Transform player)
        {
            Camera cam = Camera.main;
            if (cam == null) return false;

            Vector3 toObject = (transform.position - cam.transform.position).normalized;
            float dot = Vector3.Dot(cam.transform.forward, toObject);

            // dot ~ 1.0 = กล้องมองตรงไปที่วัตถุ, dot ~ 0.0 = มองด้านข้าง
            return dot > 0.9f; // ปรับความแม่นตามต้องการ
        }


        public virtual void Interact()
        {
            Debug.Log($"Interact: {name}");
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }

}