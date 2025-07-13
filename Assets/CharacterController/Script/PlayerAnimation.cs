using System.Linq;
using UnityEngine;

namespace ShadowMaze.MyCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator _animator;
        private static int inputXHash = Animator.StringToHash("InputX");
        private static int inputYHash = Animator.StringToHash("InputY");
        
        private static int isAttacking = Animator.StringToHash("IsAttacking");
        private static int isPlayerAction = Animator.StringToHash("IsPlayerAction");
        private int[] actionHashes;

        private static int isDie = Animator.StringToHash("IsDie");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            UpdataAnimationState();
        }

        private void UpdataAnimationState()
        {
            if (PlayerState.Instance.CurrPlayerMovementState == PlayerMovementState.Walking)
            {
                _animator.SetBool("IsRunning", false);
                _animator.SetBool("IsWalking", true);
            }
            else if (PlayerState.Instance.CurrPlayerMovementState == PlayerMovementState.Running)
            {
                _animator.SetBool("IsRunning", true);
                _animator.SetBool("IsWalking", false);
            }
            else
            {
                _animator.SetBool("IsRunning", false);
                _animator.SetBool("IsWalking", false);
            }
            
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