using UnityEngine;

public class PuzzleTrapTrigger : Puzzle
{
    public TimelineTrigger timelineTrigger; // ลิงก์ไปยัง TimelineTrigger
    private bool isBoxOnTrap = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && !isBoxOnTrap)
        {
            isBoxOnTrap = true;
            isFinish = true;
            Debug.Log("Finish");

            if (timelineTrigger != null)
            {
                //timelineTrigger.PlayTimeline();
            }
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
