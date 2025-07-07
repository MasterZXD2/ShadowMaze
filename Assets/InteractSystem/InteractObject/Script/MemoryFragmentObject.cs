using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class MemoryFragmentObject : InteractObject
    {
        public override void Interact()
        {
            base.Interact();
            PlayerStats playerStats = PlayerController.Instance._playerStats;
            if (playerStats.memoryFragment < playerStats.memoryFragmentMax) PlayerController.Instance._playerStats.memoryFragment++;
            Destroy(gameObject);
        }
    }

}