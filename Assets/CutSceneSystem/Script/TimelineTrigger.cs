using ShadowMaze.MyCharacterController;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timelineToPlay;

    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

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

    public void PlayTimeline()
    {
        if (timelineToPlay != null)
        {
            timelineToPlay.Play();
            isPlaying = true;

            PlayerController player = PlayerController.Instance;
            if (player)
            {
                player.OnCutScene(true);
            }
        }
        else
        {
            Debug.LogWarning("Timeline not assigned.");
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        isPlaying = false;
        PlayerController player = PlayerController.Instance;
        if (player)
        {
            player.OnCutScene(false);
        }
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
