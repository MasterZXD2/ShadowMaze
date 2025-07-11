using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace ShadowMaze.EnemyCharactor
{
    public class PuzzleEnemyAI : SimpleEnemyAi
    {
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
    }

}