using ShadowMaze.MyCharacterController;
using UnityEngine;

namespace ShadowMaze.InteractSystem
{
    public class BatteryObject : InteractObject
    {
        public override void Interact()
        {
            PlayerController.Instance._playerStats.Battery += 25;
            Destroy(gameObject);
        }
    }
}