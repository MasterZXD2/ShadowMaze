using UnityEngine;

public class DoorMF : MonoBehaviour
{
    [SerializeField] private TimelineTrigger doorTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (doorTrigger) doorTrigger.PlayTimeline();
    }
}
