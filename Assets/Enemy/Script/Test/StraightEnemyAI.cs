using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class StraightEnemyAI : TransitionEnemyAI
{
    [SerializeField] private Transform togo;
    private bool walkto = true;
    private bool lamp = false;

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
            OnFlashLight();

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
            if (!_player || isJump || isD) return;
            PlayerCheck();
            if (enemyModel)
            {
                enemyModel.transform.position = transform.position + _offset;
                enemyModel.transform.rotation = transform.rotation;
            }
            if (astop)
            {
                agent.SetDestination(transform.position);
                return;
            }
            OnFlashLight();

            float PH = PlayerController.Instance._characterController.height / 2f;
            Vector3 _tarVec = _player.position;
            _tarVec.y += PH;

            Move(PH);
            //base.Update();
        }
        isCurrentlyHitByFlashlight = false;
    }

    protected override void OnFlashLight()
    {
        if (isCurrentlyHitByFlashlight && lamp)
        {
            flashlightHitTimer += Time.deltaTime;
        }
        else
        {
            flashlightHitTimer = 0f;
        }

        if (flashlightHitTimer >= flashlightStunDuration)
        {
            TransitionToCubeForm();
            flashlightHitTimer = 0f;
        }
    }

    protected override void NormalMove(float PH)
    {
        if (astop) return;

        _lastPlayerPosition = _player.position;
        _lastPlayerPosition.y += PH;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            ChasePlayer();
        }

        if (!isAttacking)
        {
            if (transform.position == _lastGTPosition) _sameGTPositionTimer += Time.deltaTime;
            else _sameGTPositionTimer = 0f;
        }

        _lastGTPosition = transform.position;
    }

    public override void HitByFlashlight(bool b = false)
    {
        base.HitByFlashlight(b);
        lamp = b;
    }
}
