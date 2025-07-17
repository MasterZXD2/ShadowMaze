using ShadowMaze.EnemyCharactor;
using System.Collections.Generic;
using UnityEngine;

public class TheWatchingEyesArea : MonoBehaviour
{
    [SerializeField] private BoxCollider AreaSpawn;
    [field: SerializeField] private List<SimpleEnemyAi> prefabsEnemys;
    [SerializeField] private TheWatchingEyesAI boss;

}
