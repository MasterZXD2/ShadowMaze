using ShadowMaze.MyCharacterController;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class WhatToFear : MonoBehaviour
{
    public string name;
    protected float playerRange = 2f;
    protected bool PlayerInRange;
    protected Transform _player;
    
    protected virtual void Start()
    {
        FindPlayer();
    }

    protected virtual void Update()
    {
        PlayerCheck();
    }

    protected virtual void FindPlayer() => _player = PlayerController.Instance.gameObject.transform;

    protected virtual void PlayerCheck()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        PlayerInRange = dist <= playerRange;
        if (PlayerInRange)
        {
            PlayerController.Instance.BackStep();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, playerRange);
    }
}
