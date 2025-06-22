using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowMaze.MyCharacterController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerLocomotionInput : MonoBehaviour , PlayerControls.IPlayerLocomotionMapActions
    {
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        //public Dictionary<string, KeyBoardClass> keyBoardMap { get; private set; } = new Dictionary<string, KeyBoardClass>();
        public Vector2 ScrollInput { get; private set; }

        public Vector2 LookPosInput { get; private set; }

        public bool LeftClick { get; private set; }
        public bool RightClick { get; private set; }

        public bool isDie {  get; private set; }

        [SerializeField] private PlayerController _plrController;
        [SerializeField] private float _objScrollSpeed;
        private void OnEnable()
        {
            if (PlayerInputManager.instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized = cannot enable");
                return;
            }

            PlayerInputManager.instance.PlayerControls.PlayerLocomotionMap.Enable();
            PlayerInputManager.instance.PlayerControls.PlayerLocomotionMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (PlayerInputManager.instance?.PlayerControls == null)
            {
                Debug.LogError("Player controls is not initialized = cannot disable");
                return;
            }

            PlayerInputManager.instance.PlayerControls.PlayerLocomotionMap.Disable();
            PlayerInputManager.instance.PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
        }

        private void LateUpdate()
        {
            ScrollInput = Vector2.zero;
        }

        public void AttackEnd()
        {
            LeftClick = false;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (LeftClick) return;
            if (context.performed) return;
            //_plrController.LeftClick(context.started);
            LeftClick = true;
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.performed) return;
            //_plrController.RightClick(context.started);
            RightClick = true;
        }

        public void OnScrollMouse(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            Vector2 scrollInput = context.ReadValue<Vector2>();
            ScrollInput = scrollInput.normalized * _objScrollSpeed;
        }

        public void OnLookPos(InputAction.CallbackContext context)
        {
            LookPosInput = context.ReadValue<Vector2>();
        }

        /*public void OnKeyBoard(InputAction.CallbackContext context)
        {
            string name = context.control.name;
            string phaseStr = context.phase.ToString();

            if (!Enum.TryParse<KeyPhaseType>(
                phaseStr,
                true,
                out var phaseEnum))
            {
                phaseEnum = KeyPhaseType.None;
            }

            // keyBoardMap[name] = new KeyBoardClass(name,context.time,phaseEnum);
            _plrController.OnKey(new KeyBoardClass(name, context.time, phaseEnum));
        }*/
    }
}
