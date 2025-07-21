using ShadowMaze.EnemyCharactor;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    public List<GameObject> currEnemies = new List<GameObject>();
    private bool _b = false;

    public void Add(GameObject GO)
    {
        currEnemies.Add(GO);
        if (_b)
        {
            SimpleEnemyAi SE = GO.GetComponent<SimpleEnemyAi>();
            if (SE != null)
                SE.pause();
        }
    }

    public void Remove(GameObject GO)
    {
       currEnemies.Remove(GO);
    }

    public void Toggle(bool b)
    {
        _b = b;
        if (b) pause();
        else resume();
    }

    private void pause()
    {
        if (TheWatchingEyesArea.Instance) TheWatchingEyesArea.Instance.pause();
        foreach (GameObject enemy in currEnemies)
        {
            if (enemy != null)
            {
                SimpleEnemyAi SE = enemy.GetComponent<SimpleEnemyAi>();
                if (SE != null)
                    SE.pause();
            }
        }
    }

    private void resume()
    {
        if (TheWatchingEyesArea.Instance) TheWatchingEyesArea.Instance.resume();
        foreach (GameObject enemy in currEnemies)
        {
            if (enemy != null)
            {
                SimpleEnemyAi SE = enemy.GetComponent<SimpleEnemyAi>();
                if (SE != null)
                    SE.resume();
            }
        }

    }
}
