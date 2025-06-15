using System;
using UnityEngine;
using UnityEngine.Video;

namespace ShadowMaze.CutSceneSystem
{
    [RequireComponent(typeof(VideoPlayer))]
    public class CutScene : MonoBehaviour
    {
        [SerializeField] private bool loop = false;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlayed = false;
        private bool isPaused = false;

        public Action onClipEnd; // เรียก callback เมื่อคลิปจบ
        private void Awake()
        {
            if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.isLooping = loop;
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        public void Play()
        {
            if (isPlayed && !isPaused) return;

            isPlayed = true;
            videoPlayer.Play();
        }

        public void Pause()
        {
            if (!videoPlayer.isPlaying) return;

            isPaused = true;
            videoPlayer.Pause();
        }

        public void TogglePause()
        {
            if (videoPlayer == null) return;

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                isPaused = true;
            }
            else
            {
                videoPlayer.Play();
                isPaused = false;
            }
        }

        public void Stop()
        {
            videoPlayer.Stop();
            isPlayed = false;
            isPaused = false;
        }

        public void SkipTo(float time)
        {
            if (videoPlayer == null || !videoPlayer.canSetTime) return;

            videoPlayer.time = time;
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        public void Skip()
        {
            if (videoPlayer == null) return;

            // ข้ามไปจบเลย
            videoPlayer.time = videoPlayer.length - 0.1f;

            // ถ้าไม่เล่นอยู่ให้เริ่มเล่นเพื่อให้ถึงจุดจบแล้ว Trigger 'onClipEnd'
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        public void OnLoop(bool l)
        {
            videoPlayer.isLooping = l;
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (!loop)
            {
                isPlayed = false;
                isPaused = false;
            }

            onClipEnd?.Invoke();
        }

        private void OnDestroy()
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
