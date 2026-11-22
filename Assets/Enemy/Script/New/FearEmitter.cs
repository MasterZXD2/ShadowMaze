using ShadowMaze.MyCharacterController;
using UnityEngine;

public class FearEmitter : MonoBehaviour
{
    [SerializeField] private EnemyPerception perception;
    public string _name;

    protected virtual void Update()
    {
        if (perception.isPlayerInFearRange &&
            PlayerController.Instance.isFearPLayer(_name))
        {
            PlayerController.Instance.ShakeCamera(1f, 0.1f);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, perception.FearRange);
    }
}