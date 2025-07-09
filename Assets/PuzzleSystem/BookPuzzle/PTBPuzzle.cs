using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class PTBPuzzle : Puzzle
{
    [field: SerializeField] private List<GameObject> placePoints;
    [field: SerializeField] private List<string> placeNames;
    [SerializeField] private float chenkRange = .5f;
    [SerializeField] private float puzzleRange = 2f;
    private int i = 0;

    private void Update()
    {
        i = 0;
        for (int j = 0; j < placePoints.Count; j++)
        {
            GameObject go = placePoints[j];
            Collider[] colliders = Physics.OverlapSphere(go.transform.position, chenkRange);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<BookPuzzleObject>(out BookPuzzleObject bp))
                {
                    if (bp.Name == placeNames[j]) i++;
                }
            }
        }

        if (i >= placePoints.Count) isFinish = true;
        else isFinish = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (placePoints.Count > 0 ) foreach (GameObject go in placePoints) Gizmos.DrawWireSphere(go.transform.position,chenkRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, puzzleRange);
    }
}
