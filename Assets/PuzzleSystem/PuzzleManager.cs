using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : Puzzle
{
    [field: SerializeField] private List<Puzzle> puzzleList;
    public TimelineTrigger timelineTrigger;
    public string _name;

    private void Update()
    {
        if (isFinish) return;

        bool t = true;
        foreach (Puzzle puzzle in puzzleList) {
            if (!puzzle.isFinish) t = false;
        }
        if (t) { 
            isFinish = true;
            foreach(Puzzle puzzle in puzzleList)
            {
                puzzle.OnFinish();
            }
            if (timelineTrigger)
            {
                timelineTrigger.PlayTimeline();
            }

            PlayerController.Instance.wTFs.ForEach((WTF w) =>
            {
                if (w._name == _name)
                {
                    w.can = false;
                }
            });
        }
    }
}
