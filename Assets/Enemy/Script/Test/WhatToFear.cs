using ShadowMaze.MyCharacterController;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class WhatToFear : MonoBehaviour
{
    public string _name;
    public float fearRange = 2f;
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
        PlayerInRange = dist <= fearRange;
        if (PlayerInRange)
        {
            bool n = PlayerController.Instance.isFearPLayer(_name);
            if (n) {
                PlayerController.Instance.BackStep(n);
                PlayerController.Instance.ShakeCamera(1f, 0.1f);
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, fearRange);
    }
}
