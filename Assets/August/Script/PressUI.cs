using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressUI : MonoBehaviour
{
    [Header("UI")]
    public string displayName = "Press";
    public ResourceInteractUI interactUI;
    public Transform interactPoint;
    private SphereCollider sphereCollider;
    private float interactShowRange => sphereCollider.radius;
    private float interactableRange => maxDistance * .75f;
    private float maxDistance = 0;
    bool playerInRange;
    void Awake()
    {
        sphereCollider   = GetComponent<SphereCollider>();
    }
    void Start()
    {
        if (interactPoint == null)
        {
            interactPoint = transform;
        }
        // Try to find a child named "Point" and assign its transform, otherwise use this object's transform
        interactPoint = transform.Find("Point") != null ? transform.Find("Point") : transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playerInRange && other.CompareTag("Player"))
        {
            if (maxDistance == 0)
            {
                maxDistance = //Distance from ship to player
                    Vector3.Distance(interactPoint.transform.position, other.transform.position);
            }
            interactUI = InteractableUIManager.Instance.CreateResourceInteractUI(interactPoint).GetComponent<ResourceInteractUI>();
            interactUI.SetUp(displayName);
            playerInRange = true;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInRange && interactUI != null)
        {
            float distance = Vector3.Distance(other.transform.position, interactPoint.transform.position);
            if (distance > interactableRange && interactUI.interactUIState != InteractUIState.ShowInteractable)
            {
                interactUI.ReturnToShowInteractable();
            }
            else if (distance <= interactableRange && interactUI.interactUIState != InteractUIState.Interactable)
            {
                interactUI.Interactable();
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (playerInRange && other.CompareTag("Player"))
        {
            playerInRange = false;
            interactUI.HideUI();
        }
    }
}
