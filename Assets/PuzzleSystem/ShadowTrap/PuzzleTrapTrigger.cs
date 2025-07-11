using UnityEngine;

public class PuzzleTrapTrigger : Puzzle
{
    private bool isBoxOnTrap = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && !isBoxOnTrap)
        {
            isBoxOnTrap = true;
            isFinish = true;
            Debug.Log("Finish");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            isBoxOnTrap = false;
            isFinish = false;
            Debug.Log("Box removed from trap");
        }
    }
}
