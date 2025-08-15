using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class Trigger3DSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Make sure this is a 3D sound
        audioSource.spatialBlend = 1f; // Full 3D
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Play();
        }
    }
}
