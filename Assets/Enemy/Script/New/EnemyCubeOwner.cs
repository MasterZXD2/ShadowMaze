using UnityEngine;

public class EnemyCubeOwner : MonoBehaviour
{
    public CubeOnLight Owner;

    private void OnDestroy()
    {
        // Destroying a cube inside MFA may not send OnTriggerExit.
        if (Owner) Owner.OutArea();
    }
}
