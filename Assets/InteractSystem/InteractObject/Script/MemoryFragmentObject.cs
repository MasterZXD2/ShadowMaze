using ShadowMaze.CutSceneSystem;
using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class MemoryFragmentObject : InteractObject
    {
        [SerializeField] private CutScene cutScene;
        protected override void Start()
        {
            cutScene.SetVideoCamera(PlayerController.Instance._plrCamera);

            cutScene.Prepare();
            cutScene.Stop();
            cutScene.SetVideoEnd(() =>
            {
                Destroy(gameObject);
            });

            base.Start();
        }

        public override void Interact()
        {
            base.Interact();
            PlayerStats playerStats = PlayerController.Instance._playerStats;
            if (playerStats.memoryFragment < playerStats.memoryFragmentMax) PlayerController.Instance._playerStats.memoryFragment++;
            cutScene.Play();
        }
    }

}