using UnityEngine;

public class ShadowTrap : MonoBehaviour
{
    [SerializeField] private GameObject door;

    [SerializeField] private GameObject button;
    [SerializeField] private GameObject downButton;
    [SerializeField] private Vector3 scale;
    [SerializeField] private Vector3 offset;

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
        Collider[] colliders = Physics.OverlapBox(
            downButton.transform.position + offset,
            scale,
            Quaternion.identity,
            passLayer
        );
        
        foreach (Collider collider in colliders)
        {
            //Debug.Log(collider.gameObject.name);
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(downButton.transform.position + offset, scale);
    }
}
