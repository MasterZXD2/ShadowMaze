using ShadowMaze.MyCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace ShadowMaze.InteractSystem 
{
    public class InteractObject : MonoBehaviour
    {
        [SerializeField] private GameObject UI;
        [SerializeField] private float range = 2f;
        private GameObject currUI;
        private bool _playerIsInRange = false;

        private void Start()
        {
            currUI = Instantiate(UI, transform);
            currUI.transform.localPosition = Vector3.zero;

            Canvas canvas = currUI.GetComponentInChildren<Canvas>();

            Vector3 uiOffset = -transform.forward * ((transform.localScale.z / 2f) + 0.5f);
            canvas.transform.position = transform.position + uiOffset;

            if (Physics.Raycast(transform.position, uiOffset.normalized, out RaycastHit hit, uiOffset.magnitude, ~0))
            {
                canvas.transform.position = hit.point + hit.normal * 0.01f;
            }

            currUI.SetActive(false);
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (!player || !player._playerInteract) return;

            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < range * range)
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

        public virtual void Interact()
        {
            Debug.Log($"Interact: {name}");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.azure;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }

}