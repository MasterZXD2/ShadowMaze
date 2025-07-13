using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] AudioClip soundClip;
    [SerializeField] SoundType soundType;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (soundType)
            {
                case SoundType.BGM:
                    SoundManager.Instance.PlayBGM(soundClip);
                    break;
                case SoundType.SFX:
                    SoundManager.Instance.PlaySFX(soundClip);
                    break;
            }
            Destroy(gameObject); // Destroy the trigger after playing the sound
        }
    }
}
enum SoundType
{
    BGM,
    SFX
}

    
