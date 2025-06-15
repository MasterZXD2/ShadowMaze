using UnityEngine;
using UnityEngine.Video;

namespace ShadowMaze.CutSceneSystem
{
    [RequireComponent(typeof(VideoPlayer))]
    public class CutScene : MonoBehaviour
    {
        [SerializeField] private VideoClip cutsceneClip;
        [SerializeField] private bool loop = false;

        private VideoPlayer videoPlayer;
        private bool isPlayed = false;
        private bool isPaused = false;

        public System.Action onClipEnd; // เรียก callback เมื่อคลิปจบ


    }
}
