using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class PTBPuzzle : MonoBehaviour
{
    [field: SerializeField] private List<GameObject> placePoints;
    [field: SerializeField] private float chenkRange = .5f;
    [field: SerializeField] private float puzzleRange = 2f;
    private bool isFinish = false;
    private int i = 0;

    private void Update()
    {
        /*PlayerController player = PlayerController.Instance;
        if (!player) return;

        float sqrDist = (player.transform.position - transform.position).sqrMagnitude;

        var interact = player._playerInteract?._interactObject;

        if (sqrDist < puzzleRange * puzzleRange)
        {
            if (interact is BookPuzzleObject bp)
            {
                bp.inBookPuzzle = true;
                _playerIsInRange = true;
            }
        }
        else
        {
            if (_playerIsInRange && interact is BookPuzzleObject bp)
            {
                bp.inBookPuzzle = false;
                _playerIsInRange = false;
            }
        }*/
        /*Collider[] colliders = Physics.OverlapSphere(transform.position, puzzleRange);
        foreach (Collider collider in colliders) {
            if (collider.TryGetComponent<BookPuzzleObject>(out BookPuzzleObject bp)) 
            { 
                i++;
            }
        }*/
        foreach (GameObject go in placePoints)
        {
            Collider[] colliders = Physics.OverlapSphere(go.transform.position, chenkRange);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponent<BookPuzzleObject>())
                {
                    i++;
                }
            }
        }
        Debug.Log(i);
        Debug.Log(placePoints.Count);
        if (i >= placePoints.Count) { 
            isFinish = true;
            i = placePoints.Count;
        }
        else { 
            isFinish = false;
            i = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (placePoints.Count > 0 ) foreach (GameObject go in placePoints) Gizmos.DrawWireSphere(go.transform.position,chenkRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, puzzleRange);
    }
}
