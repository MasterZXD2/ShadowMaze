using UnityEngine;
using UnityEngine.Splines;

namespace ShadowMaze.EnemyCharactor
{

    public class PuzzleEnemyAI : SimpleEnemyAi
    {
        protected override void Awake()
        {
            base.Awake();
            enemyController.enabled = true;
        }

        protected override void NormalMove(float PH)
        {
            if (astop) return;
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= sightRange && isCanSeePlayer)
            {
                if (CanSeePlayer())
                {
                    _lastPlayerPosition = _player.position;
                    _lastPlayerPosition.y += PH;

                    if (dist <= attackRange)
                    {
                        AttackPlayer();
                    }
                    else
                    {
                        ChasePlayer();
                    }
                }
                else if (isLPP && IsPlayerInAwarenessRange())
                {
                    _lastPlayerPosition = _player.position;
                    _lastPlayerPosition.y += PH;
                    ChasePlayer();
                }
            }
        }

        protected override void AAP() { }
        protected override void OnFlashLight()
        {
            if (isCurrentlyHitByFlashlight)
            {
                stun = true;
                agent.SetDestination(transform.position);
                _positionSet = true;
            }
            else
            {
                stun = false;

                if (_positionSet)
                {
                    agent.SetDestination(_lastPlayerPosition);

                    Vector3 dist = transform.position - _lastPlayerPosition;
                    if (dist.magnitude < 1f)
                    {
                        enemyState.MovementSet(EnemyMovementState.Idling);
                        _positionSet = false;
                        isMovement = false;
                    }
                }
            }
        }
        public void OnPuzzle() {
            agent.SetDestination(transform.position);
            astop = true; 
        }
    }

}