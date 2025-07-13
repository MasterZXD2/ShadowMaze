using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] SoundTriggerType triggerType;
    [ShowIf("triggerType", SoundTriggerType.Control)]
    [SerializeField] UnityEvent onTriggerEnterEvent;

    [ShowIf("triggerType", SoundTriggerType.Play)]
    [SerializeField] AudioClip soundClip;
    [SerializeField] SoundType soundType;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (triggerType)
            {
                case SoundTriggerType.Control:
                    SoundManager.Instance.PlayBGM(soundClip);

                    break;
                case SoundTriggerType.Play:
                    TriggerSound();
                    break;
            }
            Destroy(gameObject); // Destroy the trigger after playing the sound
        }
    }
    public void ControlSound()
    {
        switch (triggerType)
        {
            case SoundTriggerType.Control:
                onTriggerEnterEvent.Invoke();
                break;
        }
    }
    public void TriggerSound()
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
    }
}
enum SoundType
{
    BGM,
    SFX
}
enum SoundTriggerType
{
    Control,
    Play
}

    
