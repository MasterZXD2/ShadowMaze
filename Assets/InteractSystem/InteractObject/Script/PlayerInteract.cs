using ShadowMaze.MyCharacterController;
using System;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private PlayerKeyBoardInput _playerKeyBoardInput;
        public InteractObject _interactObject { get; private set; }
        private GameObject _IOUI;
        private Action _func;

        private void Update()
        {
            if (!_IOUI) return;
            _IOUI.transform.LookAt(Camera.main.transform);
            _IOUI.transform.Rotate(0, 180f, 0);

            if (_playerKeyBoardInput.interact)
            {
                if (_func != null)
                {
                    _func.Invoke();
                }
                else if (_interactObject != null)
                {
                    _interactObject.Interact();
                }
            }
            ;
        }

        public void SetInteractObject(InteractObject interactObject, GameObject UI)
        {
            _interactObject = interactObject;
            _interactObject._name = "null";
            _IOUI = UI;
            _func = null;
        }

        public void SetInteractObjectWithOut(string Name,Action func,GameObject UI)
        {
            _interactObject = new InteractObject();
            _interactObject._name = Name;
            _IOUI = UI;
            _func = func;
        }

        public void RemoveInteractObject()
        {
            _interactObject = null;
            _IOUI = null;
            _func = null;
        }
    }
}
