using UnityEngine;

public class PuzzleTrapTrigger : MonoBehaviour
{
    private bool isBoxOnTrap = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!isBoxOnTrap)
            {
                isBoxOnTrap = true;
                Debug.Log("Finish");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            isBoxOnTrap = false;
            Debug.Log("Box removed from trap");
        }
    }
}
