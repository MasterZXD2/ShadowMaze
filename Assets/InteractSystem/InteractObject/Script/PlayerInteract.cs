using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private PlayerKeyBoardInput _playerKeyBoardInput;
        public InteractObject _interactObject { get; private set; }
        private GameObject _interactObjectGameObject;
        private GameObject _IOUI;

        private void Update()
        {
            if (!_IOUI) return;
            _IOUI.transform.LookAt(Camera.main.transform);
            _IOUI.transform.Rotate(0, 180f, 0);

            if (_playerKeyBoardInput.interact) _interactObject.Interact();
        }

        public void SetInteractObject(InteractObject interactObject, GameObject UI)
        {
            _interactObject = interactObject;
            _interactObjectGameObject = _interactObject.gameObject;
            _IOUI = UI;
        }

        public void RemoveInteractObject()
        {
            _interactObject = null;
            _interactObjectGameObject = null;
            _IOUI = null;
        }
    }
}
