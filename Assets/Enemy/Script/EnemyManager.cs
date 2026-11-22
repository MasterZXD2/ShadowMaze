using ShadowMaze.EnemyCharactor;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    public List<GameObject> currEnemies = new List<GameObject>();
    public bool _b { get; private set; } = false;

    public void Add(GameObject GO)
    {
        currEnemies.Add(GO);
        if (_b)
        {
            SimpleEnemyAi SE = GO.GetComponent<SimpleEnemyAi>();
            if (SE != null)
                SE.pause();
            EnemyController EC = GO.GetComponent<EnemyController>();
            if (EC != null)
                EC.Pause();
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
                EnemyController EC = enemy.GetComponent<EnemyController>();
                if (EC != null)
                    EC.Pause();
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
                EnemyController EC = enemy.GetComponent<EnemyController>();
                if (EC != null)
                    EC.Resume();
            }
        }

    }
}
