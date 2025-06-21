using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowMaze.MyCharacterController 
{
    public class PlayerKeyBoardInput : MonoBehaviour, PlayerControls.IPlayerKeyBoardMapActions
    {
        [SerializeField] private PlayerController _plrController;
        [SerializeField] private PlayerLight _light;

        public bool interact { get; private set; } = false;

        private void LateUpdate()
        {
            interact = false;
        }

        private void OnEnable()
        {
            if (PlayerInputManager.instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized = cannot enable");
                return;
            }

            PlayerInputManager.instance.PlayerControls.PlayerKeyBoardMap.Enable();
            PlayerInputManager.instance.PlayerControls.PlayerKeyBoardMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized = cannot disable");
                return;
            }

            PlayerInputManager.instance.PlayerControls.PlayerKeyBoardMap.Disable();
            PlayerInputManager.instance.PlayerControls.PlayerKeyBoardMap.RemoveCallbacks(this);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) return;
            _plrController.OnJump(context.started);
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.started) return;
            _plrController.OnRun(context.performed);
        }

        public void OnTestPP(InputAction.CallbackContext context)
        {
            //if (context.started) _plrController.cs.TogglePause();
        }

        public void OnTestSkip(InputAction.CallbackContext context)
        {
            //if (context.started) _plrController.cs.Skip();
        }

        public void OnFlashLight(InputAction.CallbackContext context) 
        { 
            if (context.started) _light.Toggle(); 
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started) interact = true;
        }
    }

}