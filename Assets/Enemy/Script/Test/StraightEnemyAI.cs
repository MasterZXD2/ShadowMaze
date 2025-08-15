using ShadowMaze.EnemyCharactor;
using UnityEngine;
using UnityEngine.Splines;

public class StraightEnemyAI : SimpleEnemyAi
{
    [SerializeField] private Transform togo;
    private bool walkto = true;

    public void TP(Vector3 pos)
    {
        agent.Warp(pos);
    }

    public void SetToGo(Transform transform)
    {
        togo = transform;
    }

    protected override void Update()
    {
        if (walkto && togo)
        {
            enemyState.MovementSet(EnemyMovementState.Walking);
            isMovement = true;
            agent.SetDestination(togo.position);
            Vector3 dist = transform.position - togo.position;
            if (dist.magnitude < 1f)
            {
                enemyState.MovementSet(EnemyMovementState.Idling);
                _positionSet = false;
                isMovement = false;
                walkto = false;
            }
        }
        else
        {
            base.Update();
        }
    }
}
