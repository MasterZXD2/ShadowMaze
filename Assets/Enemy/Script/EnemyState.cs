using UnityEngine;

namespace ShadowMaze.EnemyCharactor
{
    public enum EnemyMovementState
    {
        Idling,
        Walking,
        Running,
        Jumping,
        Falling,
        Strafing,
    }

    public enum EnemyActionState
    {
        Idle,
        Chasing,
        StunnedAsCube
    }

    public class EnemyState : MonoBehaviour
    {
        [field: SerializeField] public EnemyMovementState CurrEnemyMovementState { get; private set; } = EnemyMovementState.Idling;
        [field: SerializeField] public EnemyActionState CurrEnemyActionState { get; private set; } = EnemyActionState.Idle;

        public void MovementSet(EnemyMovementState enemyMovementState)
        {
            CurrEnemyMovementState = enemyMovementState;
        }

        public void ActionSet(EnemyActionState enemyActionState)
        {
            CurrEnemyActionState = enemyActionState;
        }

        public bool InGroundedState()
        {
            return CurrEnemyMovementState == EnemyMovementState.Idling ||
                CurrEnemyMovementState == EnemyMovementState.Walking ||
                CurrEnemyMovementState == EnemyMovementState.Running;
        }

        public bool InStateGroundedState(EnemyMovementState movementState)
        {
            return movementState == EnemyMovementState.Idling ||
                movementState == EnemyMovementState.Walking ||
                movementState == EnemyMovementState.Running;
        }
    }

}