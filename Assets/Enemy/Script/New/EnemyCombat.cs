using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Events;

public class EnemyCombat : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float attackCooldown = .5f;
    [SerializeField] private float attackDamage = 10f;

    public UnityEvent attackSucceeded;
    private bool isAttack = false;

    public void AttackPlayer()
    {
        if (isAttack)
            return;
        PlayerController.Instance.takeSanity(attackDamage);
        isAttack = true;
        Invoke(nameof(ResetAtteck), attackCooldown);
        attackSucceeded.Invoke();
    }
    private void ResetAtteck() => isAttack = false;
}
