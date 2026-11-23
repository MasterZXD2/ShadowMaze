using ShadowMaze.EnemyCharactor;
using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] protected EnemyController enemy;
    [SerializeField] protected EnemyPerception perception;
    [SerializeField] protected EnemyCombat combat;
    [SerializeField] protected EnemyState state;
    [SerializeField] protected EnemyMotor motor;
    [SerializeField] protected EnemyStats stats;
    [SerializeField] protected LightReaction reaction;

    protected virtual void Update()
    {
        if (enemy.IsStop || reaction.stun || motor.IsJumping)
            return;
        NormalMove();
    }

    protected virtual void NormalMove() {}

    protected Vector3 GetPlayerPosition()
    {
        PlayerController player = PlayerController.Instance;
        if (!player)
            return Vector3.zero;

        Vector3 pos = player.gameObject.transform.position;
        pos.y += player._characterController.height / 2f;

        return pos;
    }

    public void Pause()
    {
        motor.Pause();
    }

    public void Resume()
    {
        motor.Resume();
    }
}
