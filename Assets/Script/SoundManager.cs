using UnityEngine;
using PrimeTween;
public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] AudioSource bgm;
    [SerializeField] AudioSource sfx;

    public void PlayBGM(AudioClip clip)
    {
        if (bgm.clip == null && bgm.clip != clip)
        {
            // If BGM is not playing, we can directly set the clip and play it
            bgm.clip = clip;
            bgm.volume = 1f; // Ensure volume is reset to default
            bgm.loop = true; // Set BGM to loop
            bgm.Play();
        }
        else
        {
            // If BGM is already playing, we can stop it before playing a new one
            StopBGM(clip);
        }
    }
    public void StopBGM()
    {
        if (bgm != null)
        {
            //reduce the volume to zero before stopping
            Tween.Custom(
                startValue: bgm.volume,
                endValue: 0f,
                duration: 1f,
                onValueChange: v => bgm.volume = v,
                ease: Ease.Linear,
                startDelay: 0.5f
            ).OnComplete(() =>
            {
                bgm.clip = null; // clear the clip
                bgm.Stop();
                bgm.volume = 1f; // reset volume to default
            });

        }
    }
    public void StopBGM(AudioClip clip)
    {
        if (bgm != null)
        {
            //reduce the volume to zero before stopping
            Tween.Custom(
                startValue: bgm.volume,
                endValue: 0f,
                duration: 1f,
                onValueChange: v => bgm.volume = v,
                ease: Ease.Linear,
                startDelay: 0.5f
            ).OnComplete(() =>
            {
                bgm.Stop();
                bgm.volume = 1f; // reset volume to default
                bgm.clip = clip; // set the new clip
                bgm.Play(); // play the new clip
            });

        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfx != null && clip != null)
        {
            sfx.PlayOneShot(clip);
        }
    }

}
