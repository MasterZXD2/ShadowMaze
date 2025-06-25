using UnityEngine;

public class ShadowTrap : MonoBehaviour
{
    [SerializeField] private GameObject door;

    [SerializeField] private GameObject button;
    [SerializeField] private GameObject downButton;

    [field:SerializeField] private LayerMask passLayer;

    private void Update()
    {
        bool isHasThingUp = HasThingUpButton();
        if (isHasThingUp)
        {

        }
    }

    private bool HasThingUpButton()
    {
        Vector3 halfSize = downButton.transform.localScale * 0.5f;

        Collider[] colliders = Physics.OverlapBox(
            downButton.transform.position,
            halfSize,
            Quaternion.identity,
            passLayer
        );

        foreach (Collider collider in colliders)
        {
            Debug.Log(collider.gameObject.name);
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(downButton.transform.position, downButton.transform.localScale);
    }
}
