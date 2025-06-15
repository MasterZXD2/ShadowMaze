using System;
using UnityEngine;
using UnityEngine.Video;

namespace ShadowMaze.CutSceneSystem
{
    [RequireComponent(typeof(VideoPlayer))]
    public class CutScene : MonoBehaviour
    {
        [SerializeField] private bool loop = false;
        [SerializeField] private float speed = 1f;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlaying = false;
        private bool isPaused = false;

        private Action onClipEnd;
        private Action onClipStart;

        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.isLooping = loop;
            videoPlayer.playbackSpeed = speed;
            videoPlayer.started += OnVideoStart;
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        public void Play()
        {
            if (isPlaying && !isPaused) return;

            isPaused = false;
            isPlaying = true;
            videoPlayer.Play();
        }

        public void Pause()
        {
            if (!videoPlayer.isPlaying) return;

            isPaused = true;
            isPlaying = false;
            videoPlayer.Pause();
        }

        public void TogglePause()
        {
            if (videoPlayer == null) return;
            if (isPlaying) Pause();
            else Play();
        }

        public void Stop()
        {
            videoPlayer.Stop();
            isPlaying = false;
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

        public void SetLoop(bool l)
        {
            loop = l;
            videoPlayer.isLooping = l;
        }

        public void SetSpeed(float s)
        {
            speed = s;
            videoPlayer.playbackSpeed = s;
        }
        public void SetVideoCamera(Camera camera)
        {
            if (videoPlayer == null) return;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = camera;
        }
        public void SetVideoStart(Action func) => onClipStart = func;
        public void SetVideoEnd(Action func) => onClipEnd = func;
        private void OnVideoStart(VideoPlayer vp) => onClipStart?.Invoke();

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (!loop)
            {
                isPlaying = false;
                isPaused = false;
            }

            onClipEnd?.Invoke();
        }

        private void OnDestroy()
        {
            videoPlayer.started -= OnVideoStart;
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
