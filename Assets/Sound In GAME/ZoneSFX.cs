using UnityEngine;

public class ReverbToggleDemo : MonoBehaviour
{
    public AudioSource src;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            src.bypassReverbZones = !src.bypassReverbZones;
            Debug.Log("bypassReverbZones = " + src.bypassReverbZones);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            src.reverbZoneMix = src.reverbZoneMix > 0f ? 0f : 1f;
            Debug.Log("reverbZoneMix = " + src.reverbZoneMix);
        }
    }
}
