using UnityEngine;
using PrimeTween;
public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] AudioSource bgm;
    [SerializeField] AudioSource sfx;

    public void PlayBGM(AudioClip clip)
    {
        if (bgm != null && clip != null)
        {
            bgm.clip = clip;
            bgm.Play();
        }
        else if( bgm != null && bgm.isPlaying)
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
                bgm.volume,
                0f,
                1f,
                v => bgm.volume = v,
                Ease.Linear,
                startDelay: 0.5f
            ).OnComplete(() =>
            {
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
                bgm.volume,
                0f,
                1f,
                v => bgm.volume = v,
                Ease.Linear,
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
