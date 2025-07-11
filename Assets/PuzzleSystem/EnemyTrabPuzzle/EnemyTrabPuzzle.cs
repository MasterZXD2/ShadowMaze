using ShadowMaze.EnemyCharactor;
using UnityEngine;

public class EnemyTrabPuzzle : Puzzle
{
    private bool isEnemyOnTrap = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && !isEnemyOnTrap)
        {
            if (other.TryGetComponent<PuzzleEnemyAI>(out PuzzleEnemyAI pei))
            {
                pei.OnPuzzle();
                isEnemyOnTrap = true;
                isFinish = true;
                Debug.Log("Finish");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<PuzzleEnemyAI>(out PuzzleEnemyAI pei))
            {
                isEnemyOnTrap = false;
                isFinish = false;
                Debug.Log("Enemy removed from trap");
            }
        }
    }
}
