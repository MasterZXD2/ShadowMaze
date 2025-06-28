using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timelineToPlay;

    private void Start()
    {
        if (timelineToPlay != null)
        {
            timelineToPlay.stopped += OnTimelineStopped;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (timelineToPlay != null)
            {
                timelineToPlay.Play();
            }
            else
            {
                Debug.LogWarning("NAH");
            }
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (timelineToPlay != null)
        {
            timelineToPlay.stopped -= OnTimelineStopped;
        }
    }
}
