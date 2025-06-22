using System;
using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Video;

namespace ShadowMaze.CutSceneSystem
{
    [RequireComponent(typeof(VideoPlayer))]
    public class CutScene : MonoBehaviour
    {
        #region Class Veriables
        [Header("Playback Settings")]
        [SerializeField] private bool _loop = false;
        [SerializeField] private float _speed = 1f;

        [Header("References")]
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlaying = false;
        private bool isPaused = false;

        private Action onClipEnd;
        private Action onClipStart;
        private Action onClipPrepared;

        public bool IsPrepared => videoPlayer.isPrepared;
        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        #endregion

        #region Normal Function
        private void Awake()
        {
            if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
            
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = _loop;
            videoPlayer.playbackSpeed = _speed;

            videoPlayer.started += OnVideoStart;
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.errorReceived += OnVideoError;
        }

        private void OnDestroy()
        {
            videoPlayer.started -= OnVideoStart;
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }
        #endregion

        #region Control Functions
        public void Prepare()
        {
            if (!videoPlayer.clip)
            {
                Debug.LogWarning("No video clip to prepare.");
                return;
            }

            videoPlayer.Prepare();
        }

        public void Play()
        {
            if (isPlaying && !isPaused) return;
            if (!videoPlayer.clip)
            {
                Debug.LogWarning("VideoPlayer has no clip assigned!");
                return;
            }

            isPaused = false;
            isPlaying = true;
            videoPlayer.Play();
        }

        public void Pause()
        {
            if (videoPlayer == null || !videoPlayer.isPlaying) return;

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
            if (videoPlayer == null || !videoPlayer.canSetTime) return;

            double skipTime = videoPlayer.length > 0 ? videoPlayer.length - 0.1f : videoPlayer.frameCount / videoPlayer.frameRate - 0.1f;

            videoPlayer.time = skipTime;

            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }

            // ใช้ Coroutine เพื่อตรวจ frame สุดท้ายแทน loopPointReached
            StartCoroutine(WaitUntilVideoEnds());
        }

        private IEnumerator WaitUntilVideoEnds()
        {
            // รอจนกระทั่งถึงจุดสิ้นสุด
            while (videoPlayer.isPlaying && videoPlayer.time < videoPlayer.length - 0.05f)
            {
                yield return null;
            }

            isPlaying = false;
            isPaused = false;

            onClipEnd?.Invoke();
        }

        #endregion

        #region Set Veriables
        public void SetLoop(bool l)
        {
            _loop = l;
            videoPlayer.isLooping = l;
        }

        public void SetSpeed(float s)
        {
            _speed = s;
            videoPlayer.playbackSpeed = s;
        }
        public void SetVideoCamera(Camera camera)
        {
            if (videoPlayer == null) return;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = camera;
        }
        public void SetVideoStart(Action func) { if (func != null) onClipStart = func; }
        public void SetVideoEnd(Action func) { if (func != null) onClipEnd = func; }
        public void SetVideoPrepared(Action func) { if (func != null) onClipPrepared = func; }
        #endregion

        #region Event Callback
        private void OnVideoStart(VideoPlayer vp) => onClipStart?.Invoke();

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (!_loop)
            {
                isPlaying = false;
                isPaused = false;
            }

            onClipEnd?.Invoke();
        }
        private void OnVideoPrepared(VideoPlayer vp) => onClipPrepared?.Invoke();

        private void OnVideoError(VideoPlayer vp,string msg) => Debug.LogError($"Video Error: {msg}");
        #endregion
    }
}
