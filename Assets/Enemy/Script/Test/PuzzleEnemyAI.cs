using UnityEngine;

namespace ShadowMaze.EnemyCharactor
{
    public class PuzzleEnemyAI : SimpleEnemyAi
    {
        protected override void Awake()
        {
            base.Awake();
            FindingPlr = false;
        }
    }

}