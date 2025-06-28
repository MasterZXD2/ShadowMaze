using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.AI;

namespace ShadowMaze.EnemyCharactor
{
    public class SimpleEnemyAi : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private LayerMask _groundLayers, _playerLayer;

        [SerializeField] private float walkSpeed;
        [SerializeField] private float timeBetweenAttacks;

        [SerializeField] private float sightRange,attackRange;

        private Transform _player;
        private Vector3 _GTPosition;
        private bool _positionSet;
        private bool isAttack;
        private bool isPlayerInSight, isPlayerInAttack;

        private void Start()
        {
            _player = PlayerController.Instance.gameObject.transform;
            if (!agent) agent = GetComponent<NavMeshAgent>();
        }
    }

}