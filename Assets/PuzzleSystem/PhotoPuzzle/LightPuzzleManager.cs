using System.Collections.Generic;
using UnityEngine;

public class LightPuzzleManager : Puzzle
{
    private HashSet<LightPuzzleChanger> puzzles = new HashSet<LightPuzzleChanger>();
    private HashSet<LightPuzzleChanger> completedPuzzles = new HashSet<LightPuzzleChanger>();

    [SerializeField] private GameObject cutsceneTrigger; // หรือเรียก Cutscene ตรงนี้ก็ได้

    public void RegisterPuzzle(LightPuzzleChanger puzzle)
    {
        puzzles.Add(puzzle);
    }

    public void ReportCompleted(LightPuzzleChanger puzzle)
    {
        if (!puzzles.Contains(puzzle)) return;

        completedPuzzles.Add(puzzle);

        if (completedPuzzles.Count == puzzles.Count)
        {
            Debug.Log("✅ Puzzle All Completed!");
            isFinish = true;
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        if (cutsceneTrigger != null)
        {
            cutsceneTrigger.SetActive(true); // หรือเรียก Play Timeline
        }

        // หรือใช้ Timeline.Play(), LoadScene, เปิดประตู ฯลฯ
    }
}
