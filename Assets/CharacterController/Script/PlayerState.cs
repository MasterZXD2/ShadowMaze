using UnityEngine;

namespace ShadowMaze.MyCharacterController
{
    public enum PlayerMovementState
    {
        Idling,
        Walking,
        Running,
        Jumping,
        Falling,
        Strafing,
    }
    public class PlayerState : MonoBehaviour
    {
        [field: SerializeField] public PlayerMovementState CurrPlayerMovementState { get; private set; } = PlayerMovementState.Idling; 
        
        public void Set(PlayerMovementState playerMovementState)
        {
            CurrPlayerMovementState = playerMovementState;
        }

        public bool InGroundedState()
        {
            return CurrPlayerMovementState == PlayerMovementState.Idling ||
                CurrPlayerMovementState == PlayerMovementState.Walking ||
                CurrPlayerMovementState == PlayerMovementState.Running;
        }

        public bool InStateGroundedState(PlayerMovementState movementState)
        {
            return movementState == PlayerMovementState.Idling ||
                movementState == PlayerMovementState.Walking ||
                movementState == PlayerMovementState.Running;
        }
    }

}