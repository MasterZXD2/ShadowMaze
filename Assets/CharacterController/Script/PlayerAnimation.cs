using System.Linq;
using UnityEngine;

namespace ShadowMaze.MyCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerLocomotionInput _playerLocomotionInput;
        [SerializeField] private PlayerController _playerController;
        private static int inputXHash = Animator.StringToHash("InputX");
        private static int inputYHash = Animator.StringToHash("InputY");
        
        private static int isAttacking = Animator.StringToHash("IsAttacking");
        private static int isPlayerAction = Animator.StringToHash("IsPlayerAction");
        private int[] actionHashes;

        private static int isDie = Animator.StringToHash("IsDie");

        private void Awake()
        {
            if(!_playerLocomotionInput) _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            if(!_playerController) _playerController = GetComponent<PlayerController>();
            actionHashes = new int[] {isAttacking, isDie};
        }

        private void Update()
        {
            UpdataAnimationState();
        }

        private void UpdataAnimationState()
        {
            Vector2 inputTar = _playerLocomotionInput.MovementInput;
            bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

            _animator.SetBool(isDie, _playerController._isDead);

            _animator.SetFloat(inputXHash, inputTar.x);
            _animator.SetFloat(inputYHash, inputTar.y);

            _animator.SetBool(isAttacking, _playerLocomotionInput.LeftClick);
            _animator.SetBool(isPlayerAction, isPlayingAction);
        }

        public void AttackEnd()
        {
            //_playerLocomotionInput.AttackEnd();
        }

        public void CallAttack()
        {
            //_playerController.LeftClick();
        }
    }

}