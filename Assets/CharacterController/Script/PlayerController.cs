using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using static UnityEditor.PlayerSettings;

namespace ShadowMaze.MyCharacterController
{
    #region Enum

    #endregion

    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        #region Class Veriables
        [Header("Player Config")]

        [Space(10)]
        [Header("Player Components")]

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerLocomotionInput _playerLocomotionInput;
        [SerializeField] private PlayerKeyBoardInput _playerKeyBoardInput;
        [SerializeField] private PlayerState _playerState;
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private PlayerUI _playerUI;
        [SerializeField] private GameObject _playerGameObject;

        [Space(10)]
        [Header("Camera Obj")]

        [SerializeField] private GameObject FirstCam;
        [SerializeField] private GameObject ThirdCam;

        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        [Space(10)]
        [Header("Movement")]

        public float Drag = 0.1f;
        public float MovingThreshold = 0.01f;

        [Header("Walk")]

        public float WalkAcceleration = 0.25f;
        public float WalkSpeed = 4f;

        [Header("Run")]

        public float RunAcceleration = 0.5f;
        public float RunSpeed = 7f;

        [Header("Jump")]

        public float Gravity = 10f;
        public float TerminalVelo = 50f;
        public float JumpSpeed = 1.0f;
        public float InAirAcceleration = 0.15f;
        public float InAirDrag = 0.025f;

        [SerializeField] private bool holdToSpring = true;
        public bool SprintToggledOn { get; private set; }
        public bool JumpPressed { get; private set; }

        [Space(10)]
        [Header("Animation")]
        public float PlayerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.25f;

        [Space(10)]
        [Header("Camera")]

        public float LookSenseH = 0.1f;
        public float LookSenseV = 0.1f;
        public float LookLimitV = 89f;

        [Space(10)]
        [Header("Environment Details")]
        [SerializeField] private LayerMask _groundLayers;

        private Camera _plrCamera;

        private Vector2 _camRotation = Vector2.zero;
        private Vector2 _plrTarRotation = Vector2.zero;

        private bool _jumpedLastFrame = false;
        private bool _isRotatingClockwise = false;
        private float _rotatingToTargetTimer = 0f;
        private float _verticalVelo = 0f;
        private float _anitBump;
        private float _stepOffset;

        private bool _leftClick = false;
        private bool _rightClick = false;

        public bool _isDead { get; private set; } = false;
        private bool s = false;

        private bool astop = false;

        private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;

        #endregion

        #region Normal Function
        private void Awake()
        {
            if (_playerLocomotionInput == null) _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            if (_playerState == null) _playerState = GetComponent<PlayerState>();
            if (_characterController == null) _characterController = GetComponent<CharacterController>();

            _anitBump = RunSpeed;
            _stepOffset = _characterController.stepOffset;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (s || astop) return;

            _playerGameObject.transform.position = transform.position;
            MovementState();
            VerticalMovement();
            LateralMovement();

            if (_isDead) return;
            _playerGameObject.transform.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (s || astop) return;
            JumpPressed = false;
            RotationCamera();
        }
        #endregion

        #region Movement Function
        private void MovementState()
        {
            _lastMovementState = _playerState.CurrPlayerMovementState;

            bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero; //order
            bool isMovinglaterally = IsMovinglaterally(); //matter
            bool isRunning = SprintToggledOn && isMovinglaterally; //order matters

            if ((!IsGrounded() || _jumpedLastFrame) && _characterController.velocity.y > 0f)
            {
                _playerState.Set(PlayerMovementState.Jumping);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            }
            else if ((!IsGrounded() || _jumpedLastFrame) && _characterController.velocity.y <= 0f)
            {
                _playerState.Set(PlayerMovementState.Falling);
                _jumpedLastFrame = false;
                _characterController.stepOffset = 0f;
            } else
            {
                PlayerMovementState lateralState = isRunning ? PlayerMovementState.Running :
                    (isMovinglaterally || isMovementInput ? PlayerMovementState.Walking : PlayerMovementState.Idling);
                _playerState.Set(lateralState);
                _characterController.stepOffset = _stepOffset;
            }
        }

        private void VerticalMovement()
        {
            bool isGrounded = _playerState.InGroundedState();

            _verticalVelo -= Gravity * Time.deltaTime;

            if (isGrounded && _verticalVelo < 0) _verticalVelo = -_anitBump;

            if (JumpPressed && isGrounded)
            {
                _verticalVelo += _anitBump * Mathf.Sqrt(JumpSpeed * Gravity);
                _jumpedLastFrame = true;
            }

            if (_playerState.InStateGroundedState(_lastMovementState) && !isGrounded) _verticalVelo += _anitBump;

            if (Mathf.Abs(_verticalVelo) > Mathf.Abs(TerminalVelo)) _verticalVelo = -1f * Mathf.Abs(TerminalVelo);
        }
        private void LateralMovement()
        {
            bool isRunning = _playerState.CurrPlayerMovementState == PlayerMovementState.Running;
            bool isGrounded = _playerState.InGroundedState();

            float Acceleration = !isGrounded ? InAirAcceleration :
                isRunning ? RunAcceleration : WalkAcceleration;
            float Speed = isRunning || !isGrounded ? RunSpeed : WalkSpeed;

            Vector3 camForwardXZ = new Vector3(_plrCamera.transform.forward.x, 0f, _plrCamera.transform.forward.z).normalized;
            Vector3 camRightXZ = new Vector3(_plrCamera.transform.right.x, 0f, _plrCamera.transform.right.z).normalized;
            Vector3 moveDir = camRightXZ * _playerLocomotionInput.MovementInput.x + camForwardXZ * _playerLocomotionInput.MovementInput.y;

            Vector3 moveDelta = moveDir * Acceleration * Time.deltaTime;
            Vector3 newVelo = _characterController.velocity + moveDelta;

            float DragMagnitude = isGrounded ? Drag : InAirDrag;
            //Add drag to player
            Vector3 currDrag = newVelo.normalized * DragMagnitude * Time.deltaTime;
            newVelo = (newVelo.magnitude > DragMagnitude * Time.deltaTime) ? newVelo - currDrag : Vector3.zero;

            //Move charactor
            newVelo = Vector3.ClampMagnitude(new Vector3(newVelo.x, 0f, newVelo.z), Speed);
            newVelo.y += _verticalVelo;
            newVelo = !isGrounded ? SteepWalls(newVelo) : newVelo;
            _characterController.Move(newVelo * Time.deltaTime);
        }

        private Vector3 SteepWalls(Vector3 velo)
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_characterController,_groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _characterController.slopeLimit;

            if (!validAngle && _verticalVelo < 0f)
            {
                velo = Vector3.ProjectOnPlane(velo,normal);
            }

            return velo;
        }

        private void RotationCamera()
        {
            _camRotation.x += LookSenseH * _playerLocomotionInput.LookInput.x;
            _camRotation.y = Mathf.Clamp(_camRotation.y - LookSenseV * _playerLocomotionInput.LookInput.y, -LookLimitV, LookLimitV);

            _plrTarRotation.x += transform.eulerAngles.x + LookSenseH * _playerLocomotionInput.LookInput.x;

            float rotationTolerance = 90f;
            bool isIdling = _playerState.CurrPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = _rotatingToTargetTimer > 0f;
            if (!isIdling){ RotatePlayerToTar(); }
            else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget) { UpdateIdleRotation(rotationTolerance); }
            _plrCamera.transform.rotation = Quaternion.Euler(_camRotation.y, _camRotation.x, 0f);

            // Get angle between camera and player
            Vector3 camForwardProjectXZ = new Vector3(_plrCamera.transform.forward.x,0f,_plrCamera.transform.forward.z).normalized;
            Vector3 crossPD = Vector3.Cross(transform.forward, camForwardProjectXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossPD,transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectXZ);
        }

        private void UpdateIdleRotation(float rotationTolerance)
        {
            //ex r = -91 abs(r) = 91:
            //   abs(r)[91] > 90 = true ->:
            //      _rotatingToTargetTimer = rotateToTargetTime;
            //      _isRotatingClockwise = r[-91] > 0 = false;
            //if (_isRotatingClockwise[false] -> false && r[-91] > 0f -> false || !_isRotatingClockwise[false] -> true && r[-91] < 0f -> true) RotatePlayerToTar();

            //ex r = 91 abs(r) = 91:
            //   abs(r)[91] > 90 = true ->:
            //      _rotatingToTargetTimer = rotateToTargetTime;
            //      _isRotatingClockwise = r[91] > 0 = true;
            //if (_isRotatingClockwise[true] -> true && r[91] > 0f -> true || !_isRotatingClockwise[true] -> false && r[91] < 0f -> false) RotatePlayerToTar();

            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                _rotatingToTargetTimer = rotateToTargetTime;
                _isRotatingClockwise = RotationMismatch > rotationTolerance;
            }
            _rotatingToTargetTimer -= Time.deltaTime;

            // Rotate player
            if ((_isRotatingClockwise && RotationMismatch > 0f) || (!_isRotatingClockwise && RotationMismatch < 0f)) RotatePlayerToTar();
        }

        private void RotatePlayerToTar()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, _plrTarRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, PlayerModelRotationSpeed * Time.deltaTime);
        }

        private bool IsMovinglaterally()
        {
            Vector3 lateralVelo = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

            return lateralVelo.magnitude > MovingThreshold;
        }

        private bool IsGrounded()
        {
            bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 pos = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            bool grounded = Physics.CheckSphere(pos, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_characterController, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _characterController.slopeLimit;

            return _characterController.isGrounded && validAngle;
        }
        #endregion

        public void pause()
        {
            astop = true;
            _playerLocomotionInput.enabled = false;
            _playerKeyBoardInput.enabled = false;
        }
        public void replay() {
            astop = false;
            _playerLocomotionInput.enabled = true;
            _playerKeyBoardInput.enabled = true;
        }

        public void takeDamage(float damage)
        {
            if (_isDead) return;
            _playerStats.Health -= damage;
            //Debug.Log(Health);
            if (_playerStats.Health <= 0) die();
        }
        public void Stop()
        {
            s = true;
            gameObject.SetActive(false);
        }

        public void respawn()
        {
            s = false;
            _isDead = false;

            Debug.Log("Player has respawn.");

            replay();
            _playerUI.restart();
            _playerStats.restart();
        }

        private void die()
        {
            if (_isDead) return;
            _isDead = true;

            Debug.Log("Player has died.");

            _playerLocomotionInput.enabled = false;
            _playerKeyBoardInput.enabled = false;
            _playerUI.die();
            _playerStats.die();
        }

        #region KeyBoard Function
        /*public void OnKey(KeyBoardClass kb)
        {
            string name = kb.name;
            if (kb.phase == KeyPhaseType.Started)
            {
                if (name == "f5")
                {
                    ChangeCam();
                }
                if (name == "space")
                {
                    JumpPressed = true;
                }
            } 
            else if (kb.phase == KeyPhaseType.Performed)
            {
                if (name == "shift")
                {
                    SprintToggledOn = holdToSpring || !SprintToggledOn;
                }
            }
            else if (kb.phase == KeyPhaseType.Canceled)
            {
                if (name == "shift")
                {
                    SprintToggledOn = !holdToSpring && SprintToggledOn;
                }
                if (name == "space")
                {
                    JumpPressed = false;
                }
            }
        }*/

        public void OnJump(bool b)
        {
            JumpPressed = b;
        }

        public void OnRun(bool b)
        {
            if (b)
            {
                SprintToggledOn = holdToSpring || !SprintToggledOn;
            } else
            {
                SprintToggledOn = !holdToSpring && SprintToggledOn;
            }
        }

        #endregion
    }
}